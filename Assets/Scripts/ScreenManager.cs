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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    public void Back()
    {
        if (history.Count < 2) return;

        var prev = history.Pop();
        var cur = history.Peek();

        cur.screen.gameObject.SetActive(true);

        prev.screen.Hide();
        prev.screen.transform.GetComponentsInChildren<Image>().ToList().ForEach(i => FadeOutAnim(i, FadeDirection.Out));
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
        toBeHidden?.gameObject.SetActive(false);
        screen.Show(args);
        screen.transform.DOScale(animScale, 0f);
        screen.transform.DOScale(1.0f, animSpeed);
        screen.transform.GetComponentsInChildren<Image>().ToList().ForEach(i => FadeOutAnim(i, FadeDirection.In));
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

    //TODO investigate, this may be the cause of extreme stutter on mobile
    private void FadeOutAnim(Image image, FadeDirection dir)
    {
        //TEMP causes freeze on phone
        return;

        var oldTransparency = image.color.a;
        if (dir == FadeDirection.In) 
            image.DOFade(0f, 0f).OnComplete(() => image.DOFade(oldTransparency, animSpeed));
        else if (dir == FadeDirection.Out)
            image.DOFade(0f, animSpeed).OnComplete(() => image.DOFade(oldTransparency, 0f));
    }
}
