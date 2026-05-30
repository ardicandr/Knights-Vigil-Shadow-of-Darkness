using UnityEngine;

public class WindmillRotate : MonoBehaviour
{
    [Header("Kecepatan Putar")]
    public float speed = 30f;

    [Header("Pilih Sumbu (Isi angka 1 pada sumbu yang diinginkan)")]
    // Kita gunakan Vector3 agar kamu bisa setting langsung di Inspector
    // Misal: X:0, Y:0, Z:1
    public Vector3 rotationAxis = new Vector3(0, 0, 1);

    void Update()
    {
        // Space.Self memastikan baling-baling berputar pada porosnya sendiri 
        // meskipun bangunan windmill-nya miring atau diputar-putar.
        transform.Rotate(rotationAxis * speed * Time.deltaTime, Space.Self);
    }
}