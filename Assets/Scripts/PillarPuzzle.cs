using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PilarPuzzle : MonoBehaviour, IInteractable
{
    [Header("Identitas Pilar")]
    public int idPilarUnique;

    [Header("Pengaturan Tekstur Simbol 3D")]
    public Renderer meshSimbolRenderer; 
    public Texture teksturSimbolOff;    
    public Texture teksturSimbolOn;     

    [Header("Warna Cahaya Menyala (HDR)")]
    [ColorUsage(true, true)] 
    public Color warnaGlow = Color.white * 3f; 

    [Header("Audio Settings (Dark Fantasy)")]
    [SerializeField] private AudioClip clickSound;

    private PuzzleManager manager;
    private bool sudahDipukul = false;
    private Material matSimbol;
    private AudioSource audioSource;

    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    private static readonly int EmissionMapID = Shader.PropertyToID("_EmissionMap");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        manager = FindObjectOfType<PuzzleManager>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        

        if (meshSimbolRenderer != null)
        {
            matSimbol = meshSimbolRenderer.material;
            matSimbol.EnableKeyword("_EMISSION");
            ResetPilar();
        }
    }

    public void Interact()
    {
        if (manager != null && manager.IsSedangMengulang()) return;

        if (!sudahDipukul)
        {
            sudahDipukul = true;
            
            if (audioSource != null && clickSound != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            manager.PilarDipukul(idPilarUnique);
            AktifkanEfekGlow();
        }
    }

    void AktifkanEfekGlow()
    {
        if (matSimbol != null)
        {
            matSimbol.EnableKeyword("_EMISSION");
            matSimbol.SetTexture(BaseMapID, teksturSimbolOn); 
            matSimbol.SetTexture(EmissionMapID, teksturSimbolOn); 
            matSimbol.SetColor(EmissionColorID, warnaGlow);
            
            DynamicGI.SetEmissive(meshSimbolRenderer, warnaGlow);
            RendererExtensions.UpdateGIMaterials(meshSimbolRenderer);
        }
    }

    public void ResetPilar()
    {
        sudahDipukul = false;
        if (matSimbol != null)
        {
            matSimbol.SetTexture(BaseMapID, teksturSimbolOff); 
            matSimbol.SetTexture(EmissionMapID, teksturSimbolOff); 
            matSimbol.SetColor(EmissionColorID, Color.black); 
        }
    }

    void OnDestroy()
    {
        if (matSimbol != null)
        {
            Destroy(matSimbol);
        }
    }
}