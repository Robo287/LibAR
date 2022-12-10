using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI;

public class ComputerListControl : MonoBehaviour
{

    [SerializeField] private GameObject ButtonTemplate;
    public GridObjectCollection ScrollList;
    public ScrollingObjectCollection ScrollProperties;
    public GameObject BackPlate;

    private readonly string labstatsURL = GlobalVars.labstatsUrl;
    private readonly string labstatsKey = GlobalVars.labstatsKey;
    // private readonly string labstatsURL = "https://api.labstats.com";
    // private readonly string labstatsKey = "4504d01b-7154-4ea7-99e4-b797c30beaf4";

    void Start()
    {
        StartCoroutine(APIHandler());
    }

    IEnumerator APIHandler()
    {
        using (UnityWebRequest getGroups = UnityWebRequest.Get(labstatsURL + "/groups/"))
        {
            Debug.Log("Getting list of computer groups!");
            getGroups.SetRequestHeader("Authorization", "Bearer " + labstatsKey); // authenticate request
            yield return getGroups.SendWebRequest();
            // var groups = getGroups.downloadHandler.text;
            var node = JSONNode.Parse(getGroups.downloadHandler.text);
            var groups = node.AsArray;
            foreach (JSONNode group in groups)
            {
                var id = group[0];
                var name = group[3];
                // Debug.Log("Group: " + id + " is named: " + name);
                GameObject button = Instantiate(ButtonTemplate) as GameObject;
                button.SetActive(true);
                // button.GetComponent<ButtonControl>().SetText(id + System.Environment.NewLine + name);
                button.transform.SetParent(ButtonTemplate.transform.parent, false);
            }
            ScrollList.UpdateCollection();
            ScrollProperties.UpdateContent();
            BackPlate.SetActive(true);
        }
    }
}