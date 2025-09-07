using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SPUI : MonoBehaviour
{
    public Image[] spSquares; // Assign 5 squares in inspector
    public float fadeDuration = 0.2f;

    public void UpdateSP(int currentSP)
    {
        for (int i = 0; i < spSquares.Length; i++)
        {
            bool shouldBeActive = i < currentSP;
            StartCoroutine(FadeSquare(spSquares[i], shouldBeActive));
        }
    }

    private IEnumerator FadeSquare(Image square, bool fadeIn)
    {
        Color c = square.color;
        float startAlpha = c.a;
        float targetAlpha = fadeIn ? 1f : 0.3f; // white full or faded grey
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            square.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        square.color = c;
    }
}
