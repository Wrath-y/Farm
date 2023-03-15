using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class ItemEditor : EditorWindow
{
    private ItemDataList_SO db;
    private List<ItemDetails> itemList = new List<ItemDetails>();
    private VisualTreeAsset itemRowTemplate;
    private ListView itemListView;
    
    [MenuItem("UIBuilder/ItemEditor")]
    public static void ShowExample()
    {
        ItemEditor wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        
        itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemRowTemplate.uxml");

        itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        
        LoadDB();
        GenrateListView();
    }

    private void LoadDB()
    {
        var dataList = AssetDatabase.FindAssets("ItemDataList_SO");
        if (dataList.Length == 0)
        {
            Debug.Log("ItemDataList_SO not find");
            return;
        }

        var path = AssetDatabase.GUIDToAssetPath(dataList[0]);
        db = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);

        itemList = db.ItemDetailsList;
        EditorUtility.SetDirty(db);
        
        Debug.Log(itemList[0].ItemId);
    }

    private void GenrateListView()
    {
        Func<VisualElement> makeItem = () => itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i > itemList.Count)
            {
                Debug.Log("i > itemList.Count");
                return;
            }

            if (itemList[i].itemIcon == null)
            {
                Debug.Log("itemList[i].itemIcon is nil");
                return;
            }
            e.Q<VisualElement>("Icon").style.backgroundImage = itemList[i].itemIcon.texture;
            e.Q<Label>("Name").text = itemList[i] == null ? "item is nil" : itemList[i].itemName;
        };

        itemListView.itemsSource = itemList;
        itemListView.makeItem = makeItem;
        itemListView.bindItem = bindItem;
    }
}