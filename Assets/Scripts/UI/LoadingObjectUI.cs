using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObjectUI : MonoBehaviour
{
    [Header("Referencias de Objetos")]
    [SerializeField] private RectTransform outsideRing;
    [SerializeField] private RectTransform insideSquare;
    [SerializeField] private RectTransform triangle;

    [Header("Configuración Anillo Exterior")]
    [SerializeField] private float outsideRingRotateSpeed = 30f; // Grados por segundo
    [SerializeField] private LeanTweenType outsideRingEase = LeanTweenType.linear;

    [Header("Configuración Cuadrado Interior")]
    [SerializeField] private float insideSquareRotateSpeed = -60f; // Grados por segundo (negativo para otra dirección)
    [SerializeField] private LeanTweenType insideSquareEase = LeanTweenType.linear; 

    [Header("Configuración Triángulo")]
    [SerializeField] private float triangleShiftRotateDegrees = 180f; // Rotación total en un shift
  //  [SerializeField] private float triangleShiftTime = 0.25f; // Tiempo que toma hacer el shift
    [SerializeField] private float triangleLoopDuration = 3f;
    [SerializeField] private LeanTweenType triangleShiftEase = LeanTweenType.easeOutQuad;
    void Start()
    {
        AnimateOutsideRing();
        AnimateInsideSquare();
        AnimateTriangleShift();
    }

    private void AnimateOutsideRing()
    {
        if (outsideRing == null) return;

        // Gira 360 grados sobre el eje Z (Vector3.forward)
        float timeToCompleteLoop = 360f / outsideRingRotateSpeed;

        LeanTween.rotateAroundLocal(outsideRing.gameObject, Vector3.forward, 360f, timeToCompleteLoop)
            .setEase(outsideRingEase)
            .setIgnoreTimeScale(true)
            .setLoopCount(-1); // -1 para bucle infinito
    }

    private void AnimateInsideSquare()
    {
        if (insideSquare == null) return;

        // Gira -360 grados (sentido horario) sobre el eje Z
        float timeToCompleteLoop = 360f / Mathf.Abs(insideSquareRotateSpeed);

        LeanTween.rotateAroundLocal(insideSquare.gameObject, Vector3.forward, -360f, timeToCompleteLoop)
            .setEase(insideSquareEase)
            .setIgnoreTimeScale(true)
            .setLoopCount(-1);
    }

    private void AnimateTriangleShift()
    {
        if (triangle == null) return;

        LeanTween.rotateAroundLocal(gameObject, Vector3.forward, triangleShiftRotateDegrees, triangleLoopDuration)
             .setEase(triangleShiftEase) // El ease de un timer no importa
             .setIgnoreTimeScale(true)
             .setLoopCount(-1); // Repetir infinitamente
    }
   
}