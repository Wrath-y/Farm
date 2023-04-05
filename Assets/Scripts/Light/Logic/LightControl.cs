using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightControl : MonoBehaviour
{
    public LightPattenList_SO lightData;
    private Light2D _currentLight;
    private LightDetails _currentLightDetails;

    private void Awake()
    {
        _currentLight = GetComponent<Light2D>();
    }

    //实际切换灯光
    public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
    {
        _currentLightDetails = lightData.GetLightDetails(season, lightShift);

        if (timeDifference < Settings.lightChangeDuration)
        {
            var colorOffset = (_currentLightDetails.lightColor - _currentLight.color) / Settings.lightChangeDuration * timeDifference;
            _currentLight.color += colorOffset;
            DOTween.To(() => _currentLight.color, c => _currentLight.color = c, _currentLightDetails.lightColor, Settings.lightChangeDuration - timeDifference);
            DOTween.To(() => _currentLight.intensity, i => _currentLight.intensity = i, _currentLightDetails.lightAmount, Settings.lightChangeDuration - timeDifference);
        }
        if (timeDifference >= Settings.lightChangeDuration)
        {
            _currentLight.color = _currentLightDetails.lightColor;
            _currentLight.intensity = _currentLightDetails.lightAmount;
        }
    }
}