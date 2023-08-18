using DG.Tweening;
using UnityEngine;

public class BlastParticle : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] particlePieces;

    public void InitParticle(int cubeId, Vector3 pos)
    {
        float[] targetPosX = new float[5] { -.9f, -.4f, .1f, .4f, 1f };
        float[] targetPosY = new float[5] { -2.6f, -2.9f, -2.5f, -2.7f, -3f };
        Color particleColor = GetParticleColorById(cubeId);
        transform.position = pos;
        gameObject.SetActive(true);
        for (int i = 0; i < particlePieces.Length; i++)
        {
            Vector3 targetPos = new Vector3(targetPosX[i], targetPosY[i], 0f);
            particlePieces[i].color = particleColor;
            particlePieces[i].transform.localPosition = Vector3.zero;
            particlePieces[i].transform.localEulerAngles = Vector3.zero;
            AnimateParticlePiece(particlePieces[i], targetPos, 1f);
        }
    }

    Color GetParticleColorById(int id)
    {
        Color color = Color.white;
        switch (id)
        {
            case 0:
                color = Color.yellow;
                break;
            case 1:
                color = Color.red;
                break;
            case 2:
                color = Color.cyan;
                break;
            case 3:
                color = Color.green;
                break;
            case 4:
                color = Color.magenta;
                break;
        }

        return color;
    }

    void AnimateParticlePiece(SpriteRenderer renderer, Vector3 targetPos, float duration)
    {
        float halfDuration = duration * .5f;
        float rotationVal = 180f * -Mathf.Sign(targetPos.x);
        DOTween.Sequence()
            .Join(renderer.transform.DOLocalJump(targetPos, 2, 1, duration).SetEase(Ease.Linear))
            .Join(renderer.transform.DOLocalRotate(Vector3.forward * rotationVal, duration))
            .Join(renderer.DOFade(0f, halfDuration).SetDelay(halfDuration))
            .OnComplete(() => gameObject.SetActive(false));
    }
}
