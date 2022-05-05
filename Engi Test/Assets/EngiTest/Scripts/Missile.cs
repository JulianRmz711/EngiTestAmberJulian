using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public SphereCollider sphereCollider;
    Turret dependantTurret;

    public void StartMissileGuidence(Enemy target, Turret turret)
    {
        dependantTurret = turret;
        StopAllCoroutines();
        StartCoroutine(MissileTravel(target));
    }
    IEnumerator MissileTravel(Enemy target)
    {
        float actual = 0;
        float percentage = 0;
        float duration = 1;
        Vector3 iniPos = dependantTurret.MissileLauncher.position;
        /// slerp estandar para hacer un viaje arqueado
        while (actual < duration)
        {
            actual += .002f;
            percentage = actual / duration;
            Vector3 endPos = target.transform.position;
            Vector3 center = (iniPos + endPos) * 0.5F;
            center -= new Vector3(0, 1, 0);
            Vector3 centerToIni = iniPos - center;
            Vector3 centerToEnd = endPos - center;
            transform.position = Vector3.Slerp(centerToIni, centerToEnd, percentage);
            transform.position += center;
            yield return null;
        }
        StopCoroutine(SphereIncrement());
        StartCoroutine(SphereIncrement());
    }
    IEnumerator SphereIncrement()
    {
        float actual = 0;
        float percentage = 0;
        float duration = 1;
        float radius = 0;
        sphereCollider.radius = radius;
        while (actual < duration)
        {
            actual += .005f;
            percentage = actual / duration;
            radius = Mathf.Lerp(0, 2, percentage);
            sphereCollider.radius = radius;
            yield return null;
        }
        sphereCollider.radius = 2;
        yield return null;
        // Aqui podria "fadear" el radio del misil pero bueno las explosiones son instantaneas
        sphereCollider.radius = 0;
        transform.position = dependantTurret.MissileLauncher.position;
        yield return new WaitForSeconds(2);
        dependantTurret.missileHasExploded = true;

    }
}
