using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class UIScreen : MonoBehaviour
{
    public abstract void Show();
    public abstract void Hide();
}

public class ScreenManager : Singleton<ScreenManager>
{
    [SerializeField] private UIScreen startScreen;
    [SerializeField] private float animSpeed;
    
    private Stack<UIScreen> history = new Stack<UIScreen>();

    private new void Awake()
    {
        base.Awake();
        _ = Utils.RootPath;

        Application.targetFrameRate = 60;
        ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
        ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
    }
    void Start()
    {
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

        cur.gameObject.SetActive(true);
        
        prev.transform.DOLocalMoveX(Screen.width * 2, animSpeed).OnComplete(() => { prev.transform.DOLocalMoveX(0f, 0f).OnComplete(() => { prev.gameObject.SetActive(false); }); });
        prev.Hide();

        //cur.transform.DOLocalMoveX(Screen.width * 2, 0f).OnComplete(() => { cur.transform.DOLocalMoveX(0f, animSpeed); });
        cur.Show();
        Log("Showing screen " + cur.name);
    }

    public void ShowOther(UIScreen screen)
    {
        UIScreen toBeHidden = null;
        if (history.Count > 0)
        {
            history.Peek().Hide();
            toBeHidden = history.Peek();
        }

        history.Push(screen);
        screen.gameObject.SetActive(true);
        screen.transform.DOLocalMoveX(Screen.width * 2, 0f).OnComplete(() => { screen.transform.DOLocalMoveX(0f, animSpeed).OnComplete(() => { toBeHidden?.gameObject.SetActive(false); });  });
        screen.Show();
        Log("Showing screen " + screen.name);
    }
}
