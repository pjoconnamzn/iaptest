using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api;
using UnityEngine.UI;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using System.Text;

public class getVGs : MonoBehaviour {
    public Text vg1, vg2;

    // Use this for initialization
    void Start () {
     GameSparks.Core.GSEnumerable<ListVirtualGoodsResponse._VirtualGood> virtualGoods;
     new ListVirtualGoodsRequest()
    .SetIncludeDisabled(false)
    .Send((response) => {
        GameSparks.Core.GSData scriptData = response.ScriptData;
        virtualGoods = response.VirtualGoods;
        foreach (var i in virtualGoods)
        {
            Debug.Log(" name : " + i.Name);
            vg1.text = i.Name;
        }
    });



    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
