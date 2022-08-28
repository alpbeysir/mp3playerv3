using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;

public abstract class UIScreen : MonoBehaviour
{
    public abstract void Show(params object[] args);
    public virtual void Hide() { }
}

public class ScreenManager : Singleton<ScreenManager>
{
    [SerializeField] private UIScreen startScreen;
    [SerializeField] private float animSpeed;
    [SerializeField] private float animScale;
 
    private Stack<(UIScreen screen, object[] args)> history = new();

    private new void Awake()
    {
        base.Awake();
        _ = Utils.RootPath;

        Application.targetFrameRate = 144;
        ApplicationChrome.statusBarState = ApplicationChrome.States.TranslucentOverContent;
        ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
    }
    void Start()
    {
        if (startScreen)
            ShowOther(startScreen);

        _ = YoutubeAuth.Run();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    private void OnApplicationPause()
    {
        DB.Instance.Checkpoint();
    }

    private void OnApplicationQuit()
    {
        DB.Instance.Checkpoint();
    }

    public void Back()
    {
        if (history.Count < 2) return;

        var prev = history.Pop();
        var cur = history.Peek();

        cur.screen.gameObject.SetActive(true);

        prev.screen.Hide();
        FadeOutAnim(prev.screen.transform, FadeDirection.Out);
        prev.screen.transform.DOScale(animScale, animSpeed).OnComplete(() => { prev.screen.transform.DOScale(1.0f, 0f); prev.screen.gameObject.SetActive(false); });
        cur.screen.Show(cur.args);
    }

    public void ShowWithoutArgs(UIScreen screen) => ShowOther(screen);

    public void ShowOther(UIScreen screen, params object[] args)
    {
        UIScreen toBeHidden = null;
        if (history.Count > 0)
        {
            history.Peek().screen.Hide();
            toBeHidden = history.Peek().screen;
        }

        history.Push((screen, args));
        screen.gameObject.SetActive(true);

        if (screen.GetType() != typeof(OptionsView))
            toBeHidden?.gameObject.SetActive(false);

        screen.Show(args);
        screen.transform.DOScale(animScale, 0f);
        screen.transform.DOScale(1.0f, animSpeed);
        FadeOutAnim(screen.transform, FadeDirection.In);
    }

    public void WipeHistoryShow(UIScreen screen, params object[] args)
    {
        var toBeHidden = history.Peek().screen;
        history.Clear();
        toBeHidden?.gameObject.SetActive(false);
        ShowOther(screen, args);
    }

    enum FadeDirection
    {
        In, Out
    }

    private HashSet<Transform> runningTweens = new();

    //TODO investigate, this may be the cause of extreme stutter on mobile
    private void FadeOutAnim(Transform target, FadeDirection dir)
    {
        if (runningTweens.Contains(target)) return;

        runningTweens.Add(target);
        var l = target.GetComponentsInChildren<Graphic>();
        System.Array.ForEach(l, graphic =>
        {
            var oldTransparency = graphic.color.a;
            if (dir == FadeDirection.In)
                graphic.DOFade(0f, 0f).OnComplete(() => graphic.DOFade(oldTransparency, animSpeed));
            else if (dir == FadeDirection.Out)
                graphic.DOFade(0f, animSpeed).OnComplete(() => graphic.DOFade(oldTransparency, 0f));
        });
        runningTweens.Remove(target);
    }
}
