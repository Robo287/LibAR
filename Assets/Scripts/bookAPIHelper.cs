using System;
using System.Collections;
using System.Net;
using SimpleJSON;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public static class bookAPIHelper
{
    public static Book GetNewBook(string _title = "Law in American History", string _subject = "", string _author = "")
    {
        //string title = "Law in American History";
        string title = Uri.EscapeUriString(_title);
       
        string baseBookURL = "https://api-na.hosted.exlibrisgroup.com/primo/v1/search?vid=01FALSC_FAU:FAU&tab=LibrarySearch&scope=default_scope&q=title,exact,";
        string backBookURL = "&pfilter=rtype,exact,books&apikey=l8xx164ff846c77b491498d6a420c87569a4&limit=1";
        string url = baseBookURL + "%22" + title + "%22" + backBookURL;
        Debug.Log(url);

        var book = new Book();
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        var node = JSONNode.Parse(json);
        var nodeArray = node["docs"] as JSONArray;
        var delivery = nodeArray[0]["delivery"];
        var bestLocation = delivery["bestlocation"];
        var callNumber = bestLocation["callNumber"];
        var callNumberString = callNumber.ToString();
        var subjectPart = callNumberString.Substring(0, callNumberString.IndexOf('.'));
        var callNumberSubject = new String(subjectPart.Where(Char.IsLetter).ToArray());
        Debug.Log(callNumber);
        book.callNumber = callNumberSubject;
        return book;
    }

}