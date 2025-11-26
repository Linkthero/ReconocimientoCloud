using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Datos : MonoBehaviour
{
    public List<string> pistas;

    public static Datos instance;

    [SerializeField] private GameObject panelPistas;
    [SerializeField] private GameObject panelTextoPistas;

    public List<string> escaneados;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pistas = new List<string>();
        escaneados = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void mostrarPistasObtenidas()
    {
        int i = 0;
        foreach (var p in pistas)
        {
            panelTextoPistas.transform.GetChild(i).GetComponent<TextMeshProUGUI>().text = p;
            i++;
        }
        panelPistas.SetActive(true);
    }

    public void volver()
    {
        panelPistas.SetActive(false);
    }
}
