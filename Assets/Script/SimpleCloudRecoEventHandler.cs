using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Vuforia;
using System.Collections;
/*

    Script que:
        - Genera una opción aleatoriamente (planetas) y lo muestra en pantalla
        - Reconoce imágenes del cloud y obtiene el nombre (TargetName)
        - Comprueba si la imagen coincide con la mostrada en el texto



*/
public class SimpleCloudRecoEventHandler : MonoBehaviour
{
    CloudRecoBehaviour mCloudRecoBehaviour;
    bool mIsScanning = false;
    string mTargetMetadata = "";
      [SerializeField] TextMeshPro m_Object;


    public ImageTargetBehaviour ImageTargetTemplate;

    public void generaPlanetaBuscar()
    {
     
        string[] planetas= {"Tierra","Jupiter"};
        
        m_Object.text = planetas[Random.Range(0, planetas.Length+1)];
    }
    void Start()
    {
          generaPlanetaBuscar();
    }
    // Register cloud reco callbacks
    void Awake()
    {
        mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        mCloudRecoBehaviour.RegisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.RegisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.RegisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.RegisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.RegisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }
    //Unregister cloud reco callbacks when the handler is destroyed
    void OnDestroy()
    {
        mCloudRecoBehaviour.UnregisterOnInitializedEventHandler(OnInitialized);
        mCloudRecoBehaviour.UnregisterOnInitErrorEventHandler(OnInitError);
        mCloudRecoBehaviour.UnregisterOnUpdateErrorEventHandler(OnUpdateError);
        mCloudRecoBehaviour.UnregisterOnStateChangedEventHandler(OnStateChanged);
        mCloudRecoBehaviour.UnregisterOnNewSearchResultEventHandler(OnNewSearchResult);
    }
     public void OnInitialized(CloudRecoBehaviour cloudRecoBehaviour)
    {
        Debug.Log("Cloud Reco initialized");
    }

    public void OnInitError(CloudRecoBehaviour.InitError initError)
    {
        Debug.Log("Cloud Reco init error " + initError.ToString());
    }

    public void OnUpdateError(CloudRecoBehaviour.QueryError updateError)
    {
        Debug.Log("Cloud Reco update error " + updateError.ToString());

    }
     public void OnStateChanged(bool scanning)
    {
        mIsScanning = scanning;

        if (scanning)
        {
            // Clear all known targets
        }
    }
      // Here we handle a cloud target recognition event
    public void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult cloudRecoSearchResult )
    {
        // Store the target metadata
        mTargetMetadata = cloudRecoSearchResult.TargetName;
        if (mTargetMetadata == m_Object.text)
            m_Object.text = "CORRECTO!";

        // Stop the scanning by disabling the behaviour
        mCloudRecoBehaviour.enabled = false;
    }
    void OnGUI() {
      // Display current 'scanning' status
      GUI.Box (new Rect(100,100,200,50), mIsScanning ? "Scanning" : "Not scanning");
      // Display metadata of latest detected cloud-target
      GUI.Box (new Rect(100,200,200,50), "Metadata: " + mTargetMetadata);
      // If not scanning, show button
      // so that user can restart cloud scanning
      if (!mIsScanning) {
          if (GUI.Button(new Rect(100,300,200,50), "Restart Scanning")) {
          // Reset Behaviour
          mCloudRecoBehaviour.enabled = true;
          mTargetMetadata="";
          }
      }
  }
}
