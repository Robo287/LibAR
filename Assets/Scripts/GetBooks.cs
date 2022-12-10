using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetBooks : MonoBehaviour
{
    public GameObject prefab;


    [SerializeField]
    private Text _direction;

    [SerializeField]
    public Transform PhysicsBeacon;
    public Transform MathBeacon;
    public Transform PsychologyBeacon;
    public Transform UsLawBeacon;


    private string bookSection;
     
    private GameObject newGameObject;
    public void InstantiatePrefab(GameObject prefab, int x, int y, int z)
    {
        newGameObject = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
    }

    public void SetGameObjectState(bool state)
    {
        newGameObject.SetActive(state);
    }


    public GameObject callGetBooks(string subject = "", string title = "Law in American History", string author = "", GameObject book = null)
    {
        var foundBook = new GameObject();
        Debug.Log($@"Getting books: Author: {author} \nSubject: {subject}\nTitle: {title}");

        ///////////////////// Book book = bookAPIHelper.GetNewBook();

        // UC 1 - the book is already provided
        // if book is not null then attach solver and return....
        // TODO:

        /*
         * UC 2 
         * 
         * Subject, title or author or any combination thereof
         * 
         * Call the FindBook API ( subject, title, author) - invoke libcal api
         *  this will return JSON Library congress code
         *  Map this code to a physical section of the Library first floor
         *      
         *          var MathSection new GameObject();
         *           // look at tutorial for adding a small cube - shape in the shape of a book
         *           // use scale property to make look like shape of book
         *          MathSection.position = new Vector (1000, 1000, 1000)
         *          attach the directionalindicator solver
         *          mathSection.addComponent<DIrectionalIndicator>(xx,xx);
         *          
         *      or
         *      
         *          Take a QR Code sticker put on a shelf
         *          Scan that code to get a location (position) Vector ( x, y, z)
         *              then attach a Spatial Anchor ( x.y.z) to real world location
         *              then you save the spatial ancor to disk
         *              You can then load the anchor, attach GameObject, attach the directional solver
         *          
         *        
         * 
         * */

        if(title == null && subject == null && author == null)
        {
            Console.WriteLine("Please submit a subject, author, or title.");
        }
        else if (title == null && subject == null && author != null)
        {
            title = author;
        }
        else if (title == null && subject != null && author == null)
        {
            title = subject;
        }
        else if (title == null && subject != null && author != null)
        {
            title = author;
        }


        bookSection = title;
        //string bookSection = "Mathematics";
        Book newBook = bookAPIHelper.GetNewBook( title, subject, author);
                
        GameObject newBookSection = new GameObject();
        newBookSection.AddComponent<SolverHandler>();
        newBookSection.AddComponent<DirectionalIndicator>();

        //we were created dynamically and probably has no direction object
        if (_direction == null) return foundBook;

        if (newBook.callNumber == "KF")
        {
            InstantiatePrefab(prefab, 85, 500, -149);
            newGameObject.GetComponent<DirectionalIndicator>().DirectionalTarget = UsLawBeacon;
            SetGameObjectState(true);
            _direction.text = "US Law";
        }
        else if (newBook.callNumber == "QC")
        {
            InstantiatePrefab(prefab, 85, 500, 149);
            newGameObject.GetComponent<DirectionalIndicator>().DirectionalTarget = PhysicsBeacon;
            SetGameObjectState(true);
            _direction.text = "Physics";
        }
        else if (newBook.callNumber == "BF")
        {
            InstantiatePrefab(prefab, 836, 500, 149);
            newGameObject.GetComponent<DirectionalIndicator>().DirectionalTarget = PsychologyBeacon;
            SetGameObjectState(true);
            _direction.text = "Psychology";
        }
        else if (newBook.callNumber == "QA")
        {
            InstantiatePrefab(prefab, 836, 500, -149);
            newGameObject.GetComponent<DirectionalIndicator>().DirectionalTarget = MathBeacon;
            SetGameObjectState(true);
            _direction.text = "Mathematics";
        }
        else
        {
            return foundBook;
        }
        foundBook = newGameObject;
        return foundBook;
    }

    public void OnButtonClick()
    {
        callGetBooks();
    }

    public void Start()
    {
        callGetBooks("", GlobalVars.getDirectionBookTitle);
    }
}