using UnityEngine;
public class DamagePopup : MonoBehaviour {
    public float floatHeight=1.5f;public float duration=1f;
    private Vector3 startPos;private float elapsed;private TextMesh textMesh;
    public void SetDamage(int damage){textMesh=gameObject.AddComponent<TextMesh>();textMesh.text="-"+damage;textMesh.fontSize=24;textMesh.color=new Color(1f,0.2f,0.2f,1f);textMesh.anchor=TextAnchor.MiddleCenter;textMesh.alignment=TextAlignment.Center;textMesh.characterSize=0.1f;startPos=transform.position;elapsed=0f;transform.localScale=Vector3.one*0.3f;}
    private void Update(){elapsed+=Time.deltaTime;float p=elapsed/duration;transform.position=startPos+Vector3.up*(floatHeight*p);transform.localScale=Vector3.one*0.3f*(1f-p*0.5f);if(textMesh!=null){Color c=textMesh.color;c.a=1f-p;textMesh.color=c;}if(Camera.main!=null)transform.forward=Camera.main.transform.forward;if(elapsed>=duration)Destroy(gameObject);}
}