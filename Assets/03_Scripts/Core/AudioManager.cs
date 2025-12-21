using System.Collections.Generic;
using UnityEngine;

public enum BGM
{
    lobby,
    COUNT
}

public enum SFX
{
    chapter_clear,
    stage_clear,
    ui_button_click,
    COUNT
}

public class AudioManager : SingletonBehaviour<AudioManager>
{
    //오디오 오브젝트를 담을 트랜스폼 변수
    public Transform BGMTrs;
    public Transform SFXTrs;
    //오디오 파일의 로드 경로 설정
    private const string AUDIO_PATH = "Audio";

    //모든 BGM 오디오 플레이어를 담을 딕셔너리 변수 선언
    private Dictionary<BGM, AudioSource> m_BGMPlayer = new Dictionary<BGM, AudioSource>();
    //BGM오디오로 재생 중인 오디오 소스 오브젝트 변수
    private AudioSource m_CurrBGMSource;
    //모든 SFX 오디오 플레이어를 담을 딕셔너리 변수 선언
    private Dictionary<SFX, AudioSource> m_SFXPlayer = new Dictionary<SFX, AudioSource>();

    protected override void Init()
    {
        base.Init();

        LoadBGMPlayer();
        LoadSFXPlayer();
    }

    //등록된 모든 BGM오디오를 순회하면서 재생 오디오소스오브젝트를 생성
    //오 트랜스폼에 생성 오디오 오브젝트를 넣고 해당 오디오를 설정
    private void LoadBGMPlayer()
    {
        for (int i = 0; i < (int)BGM.COUNT; i++)
        {
            var audioName = ((BGM)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if (!audioClip)
            {
                Logger.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = true;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = BGMTrs;
            //딕셔너리로 키와 값을 매핑하여 오디오를 저장
            m_BGMPlayer[(BGM)i] = newAudioSource;
        }
    }
    //BGM을 불러오는 함수
    private void LoadSFXPlayer()
    {
        for (int i = 0; i < (int)SFX.COUNT; i++)
        {
            var audioName = ((SFX)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if (!audioClip)
            {
                Logger.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = false;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = SFXTrs;

            m_SFXPlayer[(SFX)i] = newAudioSource;
        }
    }

    //BGM 재생 함수
    public void PlayBGM(BGM bgm)
    {
        //현재 재생되고 있는BGM오디오가 있다면
        //재생을 중지하고 null로 초기화
        if (m_CurrBGMSource)
        {
            m_CurrBGMSource.Stop();
            m_CurrBGMSource = null;
        }
        //재생하고 싶은  BGM이 등록되어 있는지 확인
        //등록되지 않았다면 에러를 발생시켜야함.
        if (!m_BGMPlayer.ContainsKey(bgm))
        {
            Logger.LogError($"Invalid clip name. {bgm}");
            return;
        }
        //재생한다면 해당 오디오소스 오브젝트를 현재재생중으로 설정하고
        //재생
        m_CurrBGMSource = m_BGMPlayer[bgm];
        m_CurrBGMSource.Play();
    }

    //BGM 일시정지
    public void PauseBGM()
    {
        if (m_CurrBGMSource) m_CurrBGMSource.Pause();
    }
    //BGM 재개
    public void ResumeBGM()
    {
        if (m_CurrBGMSource) m_CurrBGMSource.UnPause();
    }
    //BGM 정지
    public void StopBGM()
    {
        if (m_CurrBGMSource) m_CurrBGMSource.Stop();
    }
    //SFX 재생(BGM 정지 없음)
    public void PlaySFX(SFX sfx)
    {
        if (!m_SFXPlayer.ContainsKey(sfx))
        {
            Logger.LogError($"Invalid clip name. ({sfx})");
            return;
        }

        m_SFXPlayer[sfx].Play();
    }


    //효율적으로 볼륨을 조절하는 시간을 저장하고 재생되므로 필요할 때 사용할 필요가 있음.

    //Mute
    public void Mute()
    {
        foreach (var audioSourceItem in m_BGMPlayer)
        {
            audioSourceItem.Value.volume = 0f;
        }

        foreach (var audioSourceItem in m_SFXPlayer)
        {
            audioSourceItem.Value.volume = 0f;
        }
    }

    //UnMute
    public void UnMute()
    {
        foreach (var audioSourceItem in m_BGMPlayer)
        {
            audioSourceItem.Value.volume = 1f;
        }

        foreach (var audioSourceItem in m_SFXPlayer)
        {
            audioSourceItem.Value.volume = 1f;
        }
    }
}
