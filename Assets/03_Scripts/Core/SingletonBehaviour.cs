using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    // 씬 전환 시 파괴할지 여부
    protected bool m_IsDestroyOnLoad = false;

    // 싱글톤 패턴 인스턴스 변수
    protected static T m_Instance;

    public static T Instance
    {
        get { return m_Instance; }
    }

    private void Awake()
    {
            Init();
        }

    // 초기화 함수
    // 이 [SingletonBehaviour]클래스를 상속받아서 만든 클래스에서
    // 이 함수를 오버라이드해서 각각 다른 처리를 추가할 수 있도록 하기 위해서
    protected virtual void Init()
    {
        if (m_Instance == null)
        {
            m_Instance = (T)this;

            if (!m_IsDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            // null이 아닌데 init 함수를 호출하고 있다면
            // 이미 인스턴스가 있는데 다른 인스턴스를 만들려고 하는 것
            // 그러므로 이렇게 만들려고 하는 인스턴스 오브젝트를 파괴해야...
            // 그러므로 이렇게 만들려고 하는 인스턴스 오브젝트를 파괴해야...
            Destroy(gameObject);
        }
    }

    // 파괴 시 호출되는 함수
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    // 파괴 시 추가로 처리해야 할 작업을 여기서 처리
    protected virtual void Dispose()
    {
        m_Instance = null;
    }
}
