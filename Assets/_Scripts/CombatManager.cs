using UnityEngine;
public class CombatManager:MonoBehaviour{
    public static CombatManager Instance{get;private set;}
    public GameObject damagePopupPrefab;
    private Camera mainCamera;private Vector3 camOrigPos;
    private static AudioClip attackClip;private static AudioClip hitClip;
    private bool hitStopActive;
    private void Awake(){if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;GenerateAudioClips();}
    private void GenerateAudioClips(){
        if(attackClip==null)attackClip=CreateBeepClip(880f,0.08f);
        if(hitClip==null)hitClip=CreateBeepClip(220f,0.12f);
    }
    private static AudioClip CreateBeepClip(float freq,float dur){
        int sr=44100;int samples=Mathf.CeilToInt(sr*dur);
        AudioClip clip=AudioClip.Create("Beep",samples,1,sr,false);
        float[] data=new float[samples];
        for(int i=0;i<samples;i++){float t=(float)i/sr;data[i]=Mathf.Sin(2f*Mathf.PI*freq*t)*Mathf.Exp(-t*12f);}
        clip.SetData(data,0);return clip;
    }
    private void Start(){mainCamera=Camera.main;if(mainCamera!=null)camOrigPos=mainCamera.transform.position;}
    public static float CalculateDamage(float atk,float def){return Mathf.Max(atk-def,GameConfig.MinDamage);}
    public void TriggerHitEffect(GameObject target,float damage,Vector3 atkPos,bool critical=false){
        Vector3 kb=(target.transform.position-atkPos).normalized;kb.y=0;
        float dst=target.CompareTag("Player")?GameConfig.PlayerKnockback:GameConfig.EnemyKnockback;
        ApplyKnockback(target,kb,dst);
        if(target.CompareTag("Player")){
            StartCoroutine(ScreenShake());
            GameFeelVFXManager.Instance.PlayPlayerDamaged(target,kb);
        }else{
            GameFeelVFXManager.Instance.PlayEnemyHit(target,atkPos,critical);
        }
        SpawnDamagePopup(target.transform.position,(int)damage,critical);
        PlayHitSound(target.transform.position);
    }
    public void TriggerPlayerAttackImpact(Vector3 pos,bool critical,bool multiHit){
        if(critical){StartCoroutine(HitStop(0.06f,0.035f));StartCoroutine(ScreenShake(0.11f,0.13f));}
        else{StartCoroutine(HitStop(0.035f,0.12f));StartCoroutine(ScreenShake(0.055f,0.08f));}
        if(multiHit)StartCoroutine(ScreenShake(0.07f,0.1f));
        VisualEffectsManager.Instance.PlayHitBurst(pos+Vector3.up*0.4f,critical?new Color(1f,0.9f,0.3f,1f):new Color(1f,0.55f,0.18f,1f));
    }
    public void TriggerPlayerAttackImpact(Vector3 pos,bool critical,bool multiHit,int comboStep){
        int step=Mathf.Clamp(comboStep,1,3);
        TriggerPlayerAttackImpact(pos,critical,multiHit);
        if(step>=2)StartCoroutine(ScreenShake(0.035f*step,0.055f+step*0.015f));
        if(step>=3&&!critical)StartCoroutine(HitStop(0.045f,0.09f));
        Color color=critical?new Color(1f,0.92f,0.25f,1f):step>=3?new Color(1f,0.78f,0.18f,1f):new Color(1f,0.55f,0.18f,1f);
        VisualEffectsManager.Instance.PlayGroundPulse(pos, color, step>=3?1.6f:1.05f, step>=3?0.28f:0.2f);
    }
    public void ApplyKnockback(GameObject target,Vector3 direction,float distance){
        if(target==null)return;
        direction.y=0f;
        if(direction.sqrMagnitude<0.001f)direction=target.transform.forward;
        direction.Normalize();
        CharacterController cc=target.GetComponent<CharacterController>();
        if(cc!=null)cc.Move(direction*distance);else target.transform.position+=direction*distance;
    }
    private System.Collections.IEnumerator HitFlash(GameObject t){Renderer[]rs=t.GetComponentsInChildren<Renderer>();if(rs.Length==0)yield break;Color[]orig=new Color[rs.Length];for(int i=0;i<rs.Length;i++){if(rs[i]!=null){orig[i]=rs[i].material.color;rs[i].material.color=Color.red;}}yield return new WaitForSecondsRealtime(GameConfig.HitFlashDuration);for(int i=0;i<rs.Length;i++){if(rs[i]!=null)rs[i].material.color=orig[i];}}
    private System.Collections.IEnumerator ScreenShake(){yield return ScreenShake(GameConfig.ScreenShakeMagnitude,GameConfig.ScreenShakeDuration);}
    private System.Collections.IEnumerator ScreenShake(float magnitude,float duration){
        CameraFollow follow=mainCamera!=null?mainCamera.GetComponent<CameraFollow>():null;
        if(follow!=null){follow.AddShake(magnitude,duration);yield break;}
        if(mainCamera==null)yield break;
        float e=0f;while(e<duration){e+=Time.unscaledDeltaTime;mainCamera.transform.position=camOrigPos+new Vector3(Random.Range(-1f,1f)*magnitude,Random.Range(-1f,1f)*magnitude,0);yield return null;}if(mainCamera!=null)mainCamera.transform.position=camOrigPos;}
    private System.Collections.IEnumerator HitStop(float duration,float timeScale){
        if(hitStopActive)yield break;
        if(Time.timeScale<=0f)yield break;
        hitStopActive=true;
        float original=Time.timeScale;
        Time.timeScale=timeScale;
        float elapsed=0f;
        while(elapsed<duration){elapsed+=Time.unscaledDeltaTime;yield return null;}
        Time.timeScale=original;
        hitStopActive=false;
    }
    public void UpdateCameraOriginalPosition(Vector3 p){camOrigPos=p;}
    public void SpawnDamagePopup(Vector3 pos,int damage,bool critical=false){
        DamageTextPool.Instance.ShowDamage(pos+new Vector3(Random.Range(-0.3f,0.3f),0f,Random.Range(-0.3f,0.3f)),damage,critical);
    }
    public static void PlayAttackSound(Vector3 pos){if(attackClip!=null)AudioSource.PlayClipAtPoint(attackClip,pos,0.5f);}
    public static void PlayHitSound(Vector3 pos){if(hitClip!=null)AudioSource.PlayClipAtPoint(hitClip,pos,0.6f);}
}
