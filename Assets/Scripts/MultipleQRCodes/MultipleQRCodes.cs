using UnityEngine;
using Meta.XR.MRUtilityKit;
using TMPro;
using System.Linq;
using System;

public class MultipleQRCodes : MonoBehaviour
{
    
    [SerializeField] private GameObject trackedObjectPrefab; // empty obj, show transfrom
    [SerializeField] private GameObject trackobjInfoPrefab; //for debug
    [SerializeField] private GameObject trackedBoundsPrefab;

    
    [SerializeField] private GameObject defaultPrefab; // if no mapping qrcode, show defult prefab

    public string payLoad1; //qrcode1 payload

    

    public TextMeshProUGUI debugText;
    public TextMeshProUGUI debugText2;
    public GameObject object1;
    public void OntrackableAdded(MRUKTrackable trackable){
       
        
        
        if (trackable.TrackableType == OVRAnchor.TrackableType.QRCode && trackable.MarkerPayloadString != null)
        {
            // when recognize qrcode1 payload
            string payload = trackable.MarkerPayloadString;
            debugText.text = payload;
            if (string.Equals(
        payload?.Replace(" ", ""), 
        payLoad1?.Replace(" ", ""), 
        StringComparison.OrdinalIgnoreCase))
            {
                var trackOBJInstance = Instantiate(object1, trackable.transform);
                var trackBoundsInstance = Instantiate(trackedBoundsPrefab, trackOBJInstance.transform);
                 var boundsAreaRect = trackable.PlaneRect.Value;
            trackBoundsInstance.transform.localScale = new Vector3(boundsAreaRect.width, boundsAreaRect.height, 0.01f);
            trackBoundsInstance.transform.localPosition = new Vector3(boundsAreaRect.center.x, boundsAreaRect.center.y, 0.01f);
            }
            else
            {
                debugText2.text = "different";
            }
            
           
        }

    }
    public void OnTrackableRemoved(MRUKTrackable trackable){
        Destroy(trackable.gameObject);
        debugText.text = "removed";
    }
}
