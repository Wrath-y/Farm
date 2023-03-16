using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class ItemEditor : EditorWindow
{
    private ItemDataList_SO _db;
    private List<ItemDetails> _itemList = new List<ItemDetails>();
    private VisualTreeAsset _itemRowTemplate;
    private ScrollView _itemDetailsSection;
    private ItemDetails _activeItem;
    private Sprite _defaultIcon;
    private VisualElement _iconPreview;
    
    private ListView _itemListView;

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
        
        _itemRowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIBuilder/ItemRowTemplate.uxml");

        _defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        _itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        _itemDetailsSection = root.Q<ScrollView>("ItemDetails");
        _iconPreview = _itemDetailsSection.Q<VisualElement>("Icon");

        root.Q<Button>("AddBtn").clicked += OnAddBtnClicked;
        root.Q<Button>("DelBtn").clicked += OnDelBtnClicked;

        LoadDB();
        GenrateListView();
    }

    private void OnAddBtnClicked()
    {
        ItemDetails newItem = new ItemDetails();
        newItem.itemName = "NEW ITEM";
        newItem.itemID = 1001 + _itemList.Count;
        
        _itemList.Add(newItem);
        
        _itemListView.Rebuild();
    }
    
    private void OnDelBtnClicked()
    {
        _itemList.Remove(_activeItem);
        _itemListView.Rebuild();
        _itemDetailsSection.visible = false;
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
        _db = AssetDatabase.LoadAssetAtPath<ItemDataList_SO>(path);

        _itemList = _db.ItemDetailsList;
        EditorUtility.SetDirty(_db);
    }

    private void GenrateListView()
    {
        Func<VisualElement> makeItem = () => _itemRowTemplate.CloneTree();

        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i > _itemList.Count)
            {
                Debug.Log("i > itemList.Count");
                return;
            }
            
            e.Q<VisualElement>("Icon").style.backgroundImage = _itemList[i].itemIcon == null ? _defaultIcon.texture : _itemList[i].itemIcon.texture;
            e.Q<Label>("Name").text = _itemList[i] == null ? "item is nil" : _itemList[i].itemName;
        };

        _itemListView.itemsSource = _itemList;
        _itemListView.makeItem = makeItem;
        _itemListView.bindItem = bindItem;

        _itemListView.onSelectionChange += OnListSelectionChange;

        _itemDetailsSection.visible = false;
    }

    private void OnListSelectionChange(IEnumerable<object> selectedItem)
    {
        _activeItem = (ItemDetails)selectedItem.First();
        GetItemDetails();
        _itemDetailsSection.visible = true;
    }

    private void GetItemDetails()
    {
        _itemDetailsSection.MarkDirtyRepaint();

        _itemDetailsSection.Q<IntegerField>("ItemID").value = _activeItem.itemID;
        _itemDetailsSection.Q<IntegerField>("ItemID").RegisterValueChangedCallback(e =>
        {
            _activeItem.itemID = e.newValue;
        });
        
        _itemDetailsSection.Q<TextField>("ItemName").value = _activeItem.itemName;
        _itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(e =>
        {
            _activeItem.itemName = e.newValue;
            _itemListView.Rebuild();
        });

        _iconPreview.style.backgroundImage = _activeItem.itemIcon == null ? _defaultIcon.texture : _activeItem.itemIcon.texture;

        _itemDetailsSection.Q<ObjectField>("ItemIcon").value = _activeItem.itemIcon;
        _itemDetailsSection.Q<ObjectField>("ItemIcon").RegisterValueChangedCallback(e =>
        {
            Sprite newIcon = (Sprite)e.newValue;
            _activeItem.itemIcon = newIcon;
            _iconPreview.style.backgroundImage = newIcon == null ? _defaultIcon.texture :  newIcon.texture;
            _itemListView.Rebuild();
        });
        
        var itemTypeField = _itemDetailsSection.Q<EnumField>("ItemType");
        itemTypeField.Init(ItemType.Seed);
        itemTypeField.value = _activeItem.itemType;
        
        _itemDetailsSection.Q<TextField>("ItemName").value = _activeItem.itemName;
        _itemDetailsSection.Q<TextField>("ItemName").RegisterValueChangedCallback(e =>
        {
            _activeItem.itemName = e.newValue;
            _itemListView.Rebuild();
        });
    }
}