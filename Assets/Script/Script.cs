using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Vuforia;
using static UnityEngine.CullingGroup;
using static Vuforia.CloudRecoBehaviour;

//ORDEN PINGUINO, CIERVO, GALLINA, PERRO, TIGRE, CABALLO, GATO 
public class MetaDatos
{
    public string nombre;
    public string URL;
    public string puntuacion;
    public string info;
    public string imgcarta;

    public static MetaDatos CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<MetaDatos>(jsonString);
    }

}

public class Script : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI txtAnimalDetectado;
    [SerializeField] public TextMeshProUGUI txtInfo;
    [SerializeField] public GameObject imageTarget;
    [SerializeField] public GameObject Canva;
    [SerializeField] private GameObject prefabCarta;
    private GameObject cartaInstanciada;
    private Texture2D imgCarta;
    private GameObject modeloCartaTEMP;
    private GameObject prefabAnimal;
    CloudRecoBehaviour mCloudRecoBehaviour;
    bool mIsScanning = false;
    string mTargetMetadata = "";

    public ImageTargetBehaviour ImageTargetTemplate;

    private string[] listAnimales = { "pinguino", "ciervo", "gallina", "perro", "tigre", "caballo", "gato" };
    private List<string> animalesEscaneados = new List<string>();
    private string nextAnimal = "";
    private bool primerEscaneo = true;

    //private bool finJuego = false;

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
            txtAnimalDetectado.text = "";
            txtInfo.text = "";
        }
    }

    // Here we handle a cloud target recognition event
    public void OnNewSearchResult(CloudRecoBehaviour.CloudRecoSearchResult cloudRecoSearchResult)
    {
        MetaDatos datos;
        datos = MetaDatos.CreateFromJSON(cloudRecoSearchResult.MetaData);

        if(nextAnimal == datos.nombre || primerEscaneo)
        {
            primerEscaneo = false;
        }
        else
        {
            txtInfo.text = "ese no es el siguiente animal ";
            return;
        }
        //txtInfo.text = datos.imgcarta;
        //GameObject animal = GameObject.FindGameObjectWithTag("animal");
        //if (animal != null)
        //{
        //    Destroy(animal);
        //}

        //descargamos imagen
        StartCoroutine(GetAssetBundleImgCarta(datos.imgcarta, datos.nombre));
        //obtenemos modelo carta
        //StartCoroutine(GetAssetBundleModeloCarta(datos.modelocarta, datos.nombre));
        

        //obtenemos modelo 3d
        StartCoroutine(GetAssetBundleModelo(datos.URL));

        Canva.transform.GetChild(0).gameObject.SetActive(true);

        // Store the target metadata
        mTargetMetadata = datos.nombre;
        txtAnimalDetectado.text = datos.nombre;
        txtInfo.text = datos.info;

        if(!Datos.instance.escaneados.Contains(datos.nombre))
        {
            Datos.instance.escaneados.Add(datos.nombre);
            //añadimos la pista a la lista de pistas 
            Datos.instance.pistas.Add(datos.info);
        }
        //txt.text = cloudRecoSearchResult.TargetName;

        // Stop the scanning by disabling the behaviour
        mCloudRecoBehaviour.enabled = false;
        //reconocerImagen();

        for (int i = 0; i < listAnimales.Length; i++)
        {
            if (datos.nombre == listAnimales[i])
            {
                animalesEscaneados.Add(datos.nombre);
                nextAnimal = (i + 1 < listAnimales.Length) ? listAnimales[i + 1] : listAnimales[0]; //cogemos el siguiente animal a escanear

                if (animalesEscaneados.Count == listAnimales.Length)
                {
                    //finJuego = true;
                    txtInfo.text = "¡Felicidades! Has escaneado todos los animales.";
                }
            }


            // Build augmentation based on target 
            if (ImageTargetTemplate)
            {
                /* Enable the new result with the same ImageTargetBehaviour: */
                mCloudRecoBehaviour.EnableObservers(cloudRecoSearchResult, ImageTargetTemplate.gameObject);
            }
        }
    }


    void OnGUI()
    {
        // Display current 'scanning' status
        GUI.Box(new Rect(100, 100, 200, 50), mIsScanning ? "Scanning" : "Not scanning");
        // Display metadata of latest detected cloud-target
        GUI.Box(new Rect(100, 200, 200, 50), "Metadata: " + mTargetMetadata);
        // If not scanning, show button
        // so that user can restart cloud scanning
        if (!mIsScanning)
        {
            if (GUI.Button(new Rect(100, 300, 200, 50), "Restart Scanning"))
            {
                // Reset Behaviour
                mCloudRecoBehaviour.enabled = true;
                mTargetMetadata = "";
                borrarAnimal();
            }
        }
    }

    //public void reconocerImagen()
    //{
    //    for (int i = 0; i < ImageTargetTemplate.transform.childCount; i++)
    //    {
    //        ImageTargetTemplate.transform.GetChild(i).gameObject.SetActive(false);
    //    }

    //    switch (mTargetMetadata)
    //    {
    //        case "ciervo":
    //            ImageTargetTemplate.transform.GetChild(0).gameObject.SetActive(true);
    //            break;
    //        case "perro":
    //            ImageTargetTemplate.transform.GetChild(1).gameObject.SetActive(true);
    //            break;
    //        case "caballo":
    //            ImageTargetTemplate.transform.GetChild(2).gameObject.SetActive(true);
    //            break;
    //        case "gato":
    //            ImageTargetTemplate.transform.GetChild(3).gameObject.SetActive(true);
    //            break;
    //        case "pinguino":
    //            ImageTargetTemplate.transform.GetChild(4).gameObject.SetActive(true);
    //            break;
    //        case "tigre":
    //            ImageTargetTemplate.transform.GetChild(5).gameObject.SetActive(true);
    //            break;
    //        case "gallina":
    //            ImageTargetTemplate.transform.GetChild(6).gameObject.SetActive(true);
    //            break;
    //    }
    //}

    IEnumerator GetAssetBundleModeloCarta(string url, string n)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            txtInfo.text = "Error al cargar la carta." + www.error;
        }
        else
        {
            yield return new WaitForSeconds(1);
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            string[] allAssetNames = bundle.GetAllAssetNames();

            string gameObjectModelo = Path.GetFileNameWithoutExtension(allAssetNames[0]).ToString();

            GameObject objectFoundModelo = bundle.LoadAsset(gameObjectModelo) as GameObject;
            //objectFoundModelo.GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Assets/Resources/CartaPinguino.png");

            txtInfo.text = "obtenemos carta";
            try
            {
                prefabCarta = Instantiate(objectFoundModelo, Canva.transform.position, Canva.transform.rotation);

                prefabCarta.transform.SetParent(Canva.transform);
                txtInfo.text = "instanciamos carta";
            }
            catch (System.Exception e)
            {
                txtInfo.text = "Error al instanciar la carta." + e.Message; //es null
            }

            try
            {
                objectFoundModelo.GetComponent<RawImage>().texture = imgCarta;
                objectFoundModelo.GetComponent<TextMeshProUGUI>().text += " " + n;
                if (objectFoundModelo.GetComponent<RawImage>().texture == null)
                    txtInfo.text = " no carga imagen";
                else
                    txtInfo.text = "carga imagen";
            }
            catch (System.Exception e)
            {
                txtInfo.text = "no carga imagen";

            }
        }
    }


    IEnumerator GetAssetBundleModelo(string url)
    {
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            string[] allAssetNames = bundle.GetAllAssetNames();
            string gameObjectModelo = Path.GetFileNameWithoutExtension(allAssetNames[0]).ToString();
            GameObject objectFoundModelo = bundle.LoadAsset(gameObjectModelo) as GameObject;
            
            modeloCartaTEMP = objectFoundModelo;
            //prefabAnimal = Instantiate(objectFoundModelo, ImageTargetTemplate.transform.position , ImageTargetTemplate.transform.rotation);
            //prefabAnimal.transform.SetParent(ImageTargetTemplate.transform);
        }
    }

    IEnumerator GetAssetBundleImgCarta(string url, string n)
    {
        //txtInfo.text = "cargando img";
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            //txtInfo.text = "descargando";
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            string[] allAssetNames = bundle.GetAllAssetNames();
            string gameObject = Path.GetFileNameWithoutExtension(allAssetNames[0]).ToString();
            Texture2D ibjectFound = bundle.LoadAsset(gameObject) as Texture2D;
            imgCarta = ibjectFound;
            //txtInfo.text = "descargada";
            if (imgCarta != null)
            {
                //txtInfo.text = "no nula";
                cartaInstanciada = Instantiate(prefabCarta, Canva.transform.position, Canva.transform.rotation);
                cartaInstanciada.transform.SetParent(Canva.transform);
                cartaInstanciada.GetComponent<RawImage>().texture = imgCarta;
                
                //txtInfo.text = "CARGA IMAGEEN";
            }
        }
    }

    public void borrarAnimal()
    {
        if(prefabAnimal != null)
        {
            Destroy(modeloCartaTEMP);
            Destroy(prefabAnimal);
        }
    }

    public void QuitarPanelCarta()
    {
        Canva.transform.GetChild(0).gameObject.SetActive(false);
        Destroy(cartaInstanciada);
        prefabAnimal = Instantiate(modeloCartaTEMP, ImageTargetTemplate.transform.position, ImageTargetTemplate.transform.rotation);
        prefabAnimal.transform.SetParent(ImageTargetTemplate.transform);
    }
}