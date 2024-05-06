using System.Collections.Generic;

[System.Serializable]
public class StateMachine<TOwner>
{

    public TOwner Owner { get; }

    //基底クラスをここで定義
    public abstract class BaseState
    {
        public StateMachine<TOwner> stateMachine;
        public TOwner owner => stateMachine.Owner;
        public Dictionary<int, BaseState> transitions = new Dictionary<int, BaseState>();
        public virtual void Entry() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    //どこからでも遷移してほしいイベントがあった時に使う
    public class AnyState : BaseState { }

    public BaseState CurrentState { get; private set; }

    //ステートのリスト、直接は使わない、遷移を設定するときに渡す用
    private LinkedList<BaseState> stateList = new LinkedList<BaseState>();

    //コンストラクタ
    public StateMachine(TOwner owner)
    {
        this.Owner = owner;
    }

    //リストにステートを追加
    private T AddStateList<T>() where T : BaseState,new()
    {
        T addState = new T();
        addState.stateMachine = this;
        stateList.AddLast(addState);
        return addState;
    }
    //リストからステートを探す
    private T SearchState<T>() where T : BaseState, new()
    {
        foreach (BaseState state in stateList)
        {
            if (state is T re)
            {
                return re;
            }
        }
        //ステートクラスが見つからなかったら追加する
        return AddStateList<T>();
    }

    //ステート遷移を追加
    public void AddTransition<TFrom,TTO>(int eventID) where TFrom:BaseState,new() where TTO:BaseState,new()
    {
        TFrom from = SearchState<TFrom>();
        if (from.transitions.ContainsKey(eventID)) return;
        TTO to = SearchState<TTO>();
        from.transitions.Add(eventID, to);
    }
    //どこからでも遷移できるイベントの追加
    public void AddAnyTransition<TTo>(int eventID) where TTo : BaseState, new()
    {
        AddTransition<AnyState, TTo>(eventID);
    }

    //イベントIDを発行し、現在のステートにそのイベントがあったら遷移
    //どこからでも遷移できるイベントの場合はAnyStateからイベントを探す
    public void Dispatch(int eventID)
    {
        BaseState to;
        if (!CurrentState.transitions.TryGetValue(eventID,out to))
        {
            if (!SearchState<AnyState>().transitions.TryGetValue(eventID, out to)) return;
        }
        TransitionTo(to);
    }

    //このステートマシンの最初のステートを決める
    public void Initialize<T>() where T:BaseState,new()
    {

        CurrentState = SearchState<T>();
    }

    //ステートのアップデート処理
    public void OnUpdate()
    {
        CurrentState.Update();
    }

    //ステートの直接的な遷移
    private void TransitionTo(BaseState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Entry();
    }
}
