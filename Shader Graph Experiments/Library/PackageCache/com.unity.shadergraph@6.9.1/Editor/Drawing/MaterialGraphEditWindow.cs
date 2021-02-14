using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor.Graphing;
using UnityEditor.Graphing.Util;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;

using UnityEditor.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.Drawing
{
    class MaterialGraphEditWindow : EditorWindow
    {
        [SerializeField]
        string m_Selected;

        [SerializeField]
        GraphObject m_GraphObject;

        [NonSerialized]
        bool m_HasError;

        [NonSerialized]
        HashSet<string> m_ChangedFileDependencies = new HashSet<string>();

        ColorSpace m_ColorSpace;
        RenderPipelineAsset m_RenderPipelineAsset;
        bool m_FrameAllAfterLayout;

        GraphEditorView m_GraphEditorView;

        MessageManager m_MessageManager;
        MessageManager messageManager
        {
            get { return m_MessageManager ?? (m_MessageManager = new MessageManager()); }
        }

        GraphEditorView graphEditorView
        {
            get { return m_GraphEditorView; }
            set
            {
                if (m_GraphEditorView != null)
                {
                    m_GraphEditorView.RemoveFromHierarchy();
                    m_GraphEditorView.Dispose();
                }

                m_GraphEditorView = value;
                if (m_GraphEditorView != null)
                {
                    m_GraphEditorView.saveRequested += UpdateAsset;
                    m_GraphEditorView.convertToSubgraphRequested += ToSubGraph;
                    m_GraphEditorView.showInProjectRequested += PingAsset;
                    m_GraphEditorView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;
                    this.rootVisualElement.Add(graphEditorView);
                }
            }
        }

        GraphObject graphObject
        {
            get { return m_GraphObject; }
            set
            {
                if (m_GraphObject != null)
                    DestroyImmediate(m_GraphObject);
                m_GraphObject = value;
            }
        }

        public string selectedGuid
        {
            get { return m_Selected; }
            private set { m_Selected = value; }
        }

        public string assetName
        {
            get { return titleContent.text; }
            set
            {
                titleContent.text = value;
                graphEditorView.assetName = value;
            }
        }

        void Update()
        {
            if (m_HasError)
                return;

            if (PlayerSettings.colorSpace != m_ColorSpace)
            {
                graphEditorView = null;
                m_ColorSpace = PlayerSettings.colorSpace;
            }

            if (GraphicsSettings.renderPipelineAsset != m_RenderPipelineAsset)
            {
                graphEditorView = null;
                m_RenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            }

            try
            {
                if (graphObject == null && selectedGuid != null)
                {
                    var guid = selectedGuid;
                    selectedGuid = null;
                    Initialize(guid);
                }

                if (graphObject == null)
                {
                    Close();
                    return;
                }

                var materialGraph = graphObject.graph as GraphData;
                if (materialGraph == null)
                    return;

                if (graphEditorView == null)
                {
                    messageManager.ClearAll();
                    materialGraph.messageManager = messageManager;
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedGuid));
                    graphEditorView = new GraphEditorView(this, materialGraph, messageManager)
                    {
                        viewDataKey = selectedGuid,
                        assetName = asset.name.Split('/').Last()
                    };
                    m_ColorSpace = PlayerSettings.colorSpace;
                    m_RenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
                    graphObject.Validate();
                }

                if (m_ChangedFileDependencies.Count > 0 && graphObject != null && graphObject.graph != null)
                {
                    foreach (var subGraphNode in graphObject.graph.GetNodes<SubGraphNode>())
                    {
                        subGraphNode.Reload(m_ChangedFileDependencies);
                    }
                    foreach (var customFunctionNode in graphObject.graph.GetNodes<CustomFunctionNode>())
                    {
                        customFunctionNode.Reload(m_ChangedFileDependencies);
                    }

                    m_ChangedFileDependencies.Clear();
                }

                if (graphObject.wasUndoRedoPerformed)
                {
                    graphEditorView.HandleGraphChanges();
                    graphObject.graph.ClearChanges();
                    graphObject.HandleUndoRedo();
                }

                graphEditorView.HandleGraphChanges();
                graphObject.graph.ClearChanges();
            }
            catch (Exception e)
            {
                m_HasError = true;
                m_GraphEditorView = null;
                graphObject = null;
                Debug.LogException(e);
                throw;
            }
        }

        public void ReloadSubGraphsOnNextUpdate(List<string> changedFiles)
        {
            foreach (var changedFile in changedFiles)
            {
                m_ChangedFileDependencies.Add(changedFile);
            }
        }

        void OnEnable()
        {
            this.SetAntiAliasing(4);
        }

        void OnDisable()
        {
            graphEditorView = null;
            messageManager.ClearAll();
        }

        void OnDestroy()
        {
            if (graphObject != null)
            {
                string nameOfFile = AssetDatabase.GUIDToAssetPath(selectedGuid);
                if (graphObject.isDirty && EditorUtility.DisplayDialog("Shader Graph Has Been Modified", "Do you want to save the changes you made in the Shader Graph?\n" + nameOfFile + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
                    UpdateAsset();
                Undo.ClearUndo(graphObject);
                DestroyImmediate(graphObject);
            }

            graphEditorView = null;
        }

        public void PingAsset()
        {
            if (selectedGuid != null)
            {
                var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(asset);
            }
        }

        public void UpdateAsset()
        {
            if (selectedGuid != null && graphObject != null)
            {
                var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
                if (string.IsNullOrEmpty(path) || graphObject == null)
                    return;

                UpdateShaderGraphOnDisk(path);

                graphObject.isDirty = false;
            }
        }

        public void ToSubGraph()
        {
            var graphView = graphEditorView.graphView;

            var path = EditorUtility.SaveFilePanelInProject("Save Sub Graph", "New Shader Sub Graph", ShaderSubGraphImporter.Extension, "");
            path = path.Replace(Application.dataPath, "Assets");
            if (path.Length == 0)
                return;

            graphObject.RegisterCompleteObjectUndo("Convert To Subgraph");

            var nodes = graphView.selection.OfType<IShaderNodeView>().Where(x => !(x.node is PropertyNode || x.node is SubGraphOutputNode)).Select(x => x.node).Where(x => x.allowedInSubGraph).ToArray();
            var bounds = Rect.MinMaxRect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (var node in nodes)
            {
                var center = node.drawState.position.center;
                bounds = Rect.MinMaxRect(
                        Mathf.Min(bounds.xMin, center.x),
                        Mathf.Min(bounds.yMin, center.y),
                        Mathf.Max(bounds.xMax, center.x),
                        Mathf.Max(bounds.yMax, center.y));
            }
            var middle = bounds.center;
            bounds.center = Vector2.zero;

            // Collect the property nodes and get the corresponding properties
            var propertyNodeGuids = graphView.selection.OfType<IShaderNodeView>().Where(x => (x.node is PropertyNode)).Select(x => ((PropertyNode)x.node).propertyGuid);
            var metaProperties = graphView.graph.properties.Where(x => propertyNodeGuids.Contains(x.guid));

            var copyPasteGraph = new CopyPasteGraph(
                    graphView.graph.assetGuid,
                    graphView.selection.OfType<ShaderGroup>().Select(x => x.userData),
                    graphView.selection.OfType<IShaderNodeView>().Where(x => !(x.node is PropertyNode || x.node is SubGraphOutputNode)).Select(x => x.node).Where(x => x.allowedInSubGraph).ToArray(),
                    graphView.selection.OfType<Edge>().Select(x => x.userData as IEdge),
                    graphView.selection.OfType<BlackboardField>().Select(x => x.userData as AbstractShaderProperty),
                    metaProperties);

            var deserialized = CopyPasteGraph.FromJson(JsonUtility.ToJson(copyPasteGraph, false));
            if (deserialized == null)
                return;

            var subGraph = new GraphData { isSubGraph = true };
            subGraph.path = "Sub Graphs";
            var subGraphOutputNode = new SubGraphOutputNode();
            {
                var drawState = subGraphOutputNode.drawState;
                drawState.position = new Rect(new Vector2(bounds.xMax + 200f, 0f), drawState.position.size);
                subGraphOutputNode.drawState = drawState;
            }
            subGraph.AddNode(subGraphOutputNode);

            var nodeGuidMap = new Dictionary<Guid, Guid>();
            foreach (var node in deserialized.GetNodes<AbstractMaterialNode>())
            {
                var oldGuid = node.guid;
                var newGuid = node.RewriteGuid();
                nodeGuidMap[oldGuid] = newGuid;
                var drawState = node.drawState;
                drawState.position = new Rect(drawState.position.position - middle, drawState.position.size);
                node.drawState = drawState;
                subGraph.AddNode(node);
            }

            // figure out what needs remapping
            var externalOutputSlots = new List<IEdge>();
            var externalInputSlots = new List<IEdge>();
            foreach (var edge in deserialized.edges)
            {
                var outputSlot = edge.outputSlot;
                var inputSlot = edge.inputSlot;

                Guid remappedOutputNodeGuid;
                Guid remappedInputNodeGuid;
                var outputSlotExistsInSubgraph = nodeGuidMap.TryGetValue(outputSlot.nodeGuid, out remappedOutputNodeGuid);
                var inputSlotExistsInSubgraph = nodeGuidMap.TryGetValue(inputSlot.nodeGuid, out remappedInputNodeGuid);

                // pasting nice internal links!
                if (outputSlotExistsInSubgraph && inputSlotExistsInSubgraph)
                {
                    var outputSlotRef = new SlotReference(remappedOutputNodeGuid, outputSlot.slotId);
                    var inputSlotRef = new SlotReference(remappedInputNodeGuid, inputSlot.slotId);
                    subGraph.Connect(outputSlotRef, inputSlotRef);
                }
                // one edge needs to go to outside world
                else if (outputSlotExistsInSubgraph)
                {
                    externalInputSlots.Add(edge);
                }
                else if (inputSlotExistsInSubgraph)
                {
                    externalOutputSlots.Add(edge);
                }
            }

            // Find the unique edges coming INTO the graph
            var uniqueIncomingEdges = externalOutputSlots.GroupBy(
                    edge => edge.outputSlot,
                    edge => edge,
                    (key, edges) => new { slotRef = key, edges = edges.ToList() });

            var externalInputNeedingConnection = new List<KeyValuePair<IEdge, AbstractShaderProperty>>();

            var amountOfProps = uniqueIncomingEdges.Count();
            const int height = 40;
            const int subtractHeight = 20;
            var propPos = new Vector2(0, -((amountOfProps / 2) + height) - subtractHeight);

            foreach (var group in uniqueIncomingEdges)
            {
                var sr = group.slotRef;
                var fromNode = graphObject.graph.GetNodeFromGuid(sr.nodeGuid);
                var fromSlot = fromNode.FindOutputSlot<MaterialSlot>(sr.slotId);

                AbstractShaderProperty prop;
                switch (fromSlot.concreteValueType)
                {
                    case ConcreteSlotValueType.Texture2D:
                        prop = new TextureShaderProperty();
                        break;
                    case ConcreteSlotValueType.Texture2DArray:
                        prop = new Texture2DArrayShaderProperty();
                        break;
                    case ConcreteSlotValueType.Texture3D:
                        prop = new Texture3DShaderProperty();
                        break;
                    case ConcreteSlotValueType.Cubemap:
                        prop = new CubemapShaderProperty();
                        break;
                    case ConcreteSlotValueType.Vector4:
                        prop = new Vector4ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Vector3:
                        prop = new Vector3ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Vector2:
                        prop = new Vector2ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Vector1:
                        prop = new Vector1ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Boolean:
                        prop = new BooleanShaderProperty();
                        break;
                    case ConcreteSlotValueType.Matrix2:
                        prop = new Matrix2ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Matrix3:
                        prop = new Matrix3ShaderProperty();
                        break;
                    case ConcreteSlotValueType.Matrix4:
                        prop = new Matrix4ShaderProperty();
                        break;
                    case ConcreteSlotValueType.SamplerState:
                        prop = new SamplerStateShaderProperty();
                        break;
                    case ConcreteSlotValueType.Gradient:
                        prop = new GradientShaderProperty();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (prop != null)
                {
                    var materialGraph = (GraphData)graphObject.graph;
                    var fromPropertyNode = fromNode as PropertyNode;
                    var fromProperty = fromPropertyNode != null ? materialGraph.properties.FirstOrDefault(p => p.guid == fromPropertyNode.propertyGuid) : null;
                    prop.displayName = fromProperty != null ? fromProperty.displayName : fromSlot.concreteValueType.ToString();

                    subGraph.AddShaderProperty(prop);
                    var propNode = new PropertyNode();
                    {
                        var drawState = propNode.drawState;
                        drawState.position = new Rect(new Vector2(bounds.xMin - 300f, 0f) + propPos, drawState.position.size);
                        propPos += new Vector2(0, height);
                        propNode.drawState = drawState;
                    }
                    subGraph.AddNode(propNode);
                    propNode.propertyGuid = prop.guid;

                    foreach (var edge in group.edges)
                    {
                        subGraph.Connect(
                            new SlotReference(propNode.guid, PropertyNode.OutputSlotId),
                            new SlotReference(nodeGuidMap[edge.inputSlot.nodeGuid], edge.inputSlot.slotId));
                        externalInputNeedingConnection.Add(new KeyValuePair<IEdge, AbstractShaderProperty>(edge, prop));
                    }
                }
            }

            var uniqueOutgoingEdges = externalInputSlots.GroupBy(
                    edge => edge.outputSlot,
                    edge => edge,
                    (key, edges) => new { slot = key, edges = edges.ToList() });

            var externalOutputsNeedingConnection = new List<KeyValuePair<IEdge, IEdge>>();
            foreach (var group in uniqueOutgoingEdges)
            {
                var outputNode = subGraph.outputNode as SubGraphOutputNode;

                AbstractMaterialNode node = graphView.graph.GetNodeFromGuid(group.edges[0].outputSlot.nodeGuid);
                MaterialSlot slot = node.FindSlot<MaterialSlot>(group.edges[0].outputSlot.slotId);
                var slotId = outputNode.AddSlot(slot.concreteValueType);

                var inputSlotRef = new SlotReference(outputNode.guid, slotId);

                foreach (var edge in group.edges)
                {
                    var newEdge = subGraph.Connect(new SlotReference(nodeGuidMap[edge.outputSlot.nodeGuid], edge.outputSlot.slotId), inputSlotRef);
                    externalOutputsNeedingConnection.Add(new KeyValuePair<IEdge, IEdge>(edge, newEdge));
                }
            }

            if(FileUtilities.WriteShaderGraphToDisk(path, subGraph))
                AssetDatabase.ImportAsset(path);

            var loadedSubGraph = AssetDatabase.LoadAssetAtPath(path, typeof(SubGraphAsset)) as SubGraphAsset;
            if (loadedSubGraph == null)
                return;

            var subGraphNode = new SubGraphNode();
            var ds = subGraphNode.drawState;
            ds.position = new Rect(middle - new Vector2(100f, 150f), Vector2.zero);
            subGraphNode.drawState = ds;
            graphObject.graph.AddNode(subGraphNode);
            subGraphNode.asset = loadedSubGraph;

            foreach (var edgeMap in externalInputNeedingConnection)
            {
                graphObject.graph.Connect(edgeMap.Key.outputSlot, new SlotReference(subGraphNode.guid, edgeMap.Value.guid.GetHashCode()));
            }

            foreach (var edgeMap in externalOutputsNeedingConnection)
            {
                graphObject.graph.Connect(new SlotReference(subGraphNode.guid, edgeMap.Value.inputSlot.slotId), edgeMap.Key.inputSlot);
            }

            graphObject.graph.RemoveElements(
                graphView.selection.OfType<IShaderNodeView>().Select(x => x.node).Where(x => x.allowedInSubGraph),
                Enumerable.Empty<IEdge>(),
                Enumerable.Empty<GroupData>());
            graphObject.graph.ValidateGraph();
        }

        void UpdateShaderGraphOnDisk(string path)
        {
            if (FileUtilities.WriteShaderGraphToDisk(path, graphObject.graph))
                AssetDatabase.ImportAsset(path);
        }

        public void Initialize(string assetGuid)
        {
            try
            {
                m_ColorSpace = PlayerSettings.colorSpace;
                m_RenderPipelineAsset = GraphicsSettings.renderPipelineAsset;

                var asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetGuid));
                if (asset == null)
                    return;

                if (!EditorUtility.IsPersistent(asset))
                    return;

                if (selectedGuid == assetGuid)
                    return;

                var path = AssetDatabase.GetAssetPath(asset);
                var extension = Path.GetExtension(path);
                if (extension == null)
                    return;
                // Path.GetExtension returns the extension prefixed with ".", so we remove it. We force lower case such that
                // the comparison will be case-insensitive.
                extension = extension.Substring(1).ToLowerInvariant();
                bool isSubGraph;
                switch (extension)
                {
                    case ShaderGraphImporter.Extension:
                        isSubGraph = false;
                        break;
                    case ShaderSubGraphImporter.Extension:
                        isSubGraph = true;
                        break;
                    default:
                        return;
                }

                selectedGuid = assetGuid;

                var textGraph = File.ReadAllText(path, Encoding.UTF8);
                graphObject = CreateInstance<GraphObject>();
                graphObject.hideFlags = HideFlags.HideAndDontSave;
                graphObject.graph = JsonUtility.FromJson<GraphData>(textGraph);
                graphObject.graph.assetGuid = assetGuid;
                graphObject.graph.isSubGraph = isSubGraph;
                graphObject.graph.messageManager = messageManager;
                graphObject.graph.OnEnable();
                graphObject.graph.ValidateGraph();

                graphEditorView = new GraphEditorView(this, m_GraphObject.graph, messageManager)
                {
                    viewDataKey = selectedGuid,
                    assetName = asset.name.Split('/').Last()
                };

                titleContent = new GUIContent(asset.name.Split('/').Last());

                Repaint();
            }
            catch (Exception)
            {
                m_HasError = true;
                m_GraphEditorView = null;
                graphObject = null;
                throw;
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            graphEditorView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (m_FrameAllAfterLayout)
                graphEditorView.graphView.FrameAll();
            m_FrameAllAfterLayout = false;
            foreach (var node in m_GraphObject.graph.GetNodes<AbstractMaterialNode>())
                node.Dirty(ModificationScope.Node);
        }
    }
}
