using System.Collections.Generic;

[System.Serializable]
public class StateMachine<TOwner>
{

    public TOwner Owner { get; }

    //���N���X�������Œ�`
    public abstract class BaseState
    {
        public StateMachine<TOwner> stateMachine;
        public TOwner owner => stateMachine.Owner;
        public Dictionary<int, BaseState> transitions = new Dictionary<int, BaseState>();
        public virtual void Entry() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    //�ǂ�����ł��J�ڂ��Ăق����C�x���g�����������Ɏg��
    public class AnyState : BaseState { }

    public BaseState CurrentState { get; private set; }

    //�X�e�[�g�̃��X�g�A���ڂ͎g��Ȃ��A�J�ڂ�ݒ肷��Ƃ��ɓn���p
    private LinkedList<BaseState> stateList = new LinkedList<BaseState>();

    //�R���X�g���N�^
    public StateMachine(TOwner owner)
    {
        this.Owner = owner;
    }

    //���X�g�ɃX�e�[�g��ǉ�
    private T AddStateList<T>() where T : BaseState,new()
    {
        T addState = new T();
        addState.stateMachine = this;
        stateList.AddLast(addState);
        return addState;
    }
    //���X�g����X�e�[�g��T��
    private T SearchState<T>() where T : BaseState, new()
    {
        foreach (BaseState state in stateList)
        {
            if (state is T re)
            {
                return re;
            }
        }
        //�X�e�[�g�N���X��������Ȃ�������ǉ�����
        return AddStateList<T>();
    }

    //�X�e�[�g�J�ڂ�ǉ�
    public void AddTransition<TFrom,TTO>(int eventID) where TFrom:BaseState,new() where TTO:BaseState,new()
    {
        TFrom from = SearchState<TFrom>();
        if (from.transitions.ContainsKey(eventID)) return;
        TTO to = SearchState<TTO>();
        from.transitions.Add(eventID, to);
    }
    //�ǂ�����ł��J�ڂł���C�x���g�̒ǉ�
    public void AddAnyTransition<TTo>(int eventID) where TTo : BaseState, new()
    {
        AddTransition<AnyState, TTo>(eventID);
    }

    //�C�x���gID�𔭍s���A���݂̃X�e�[�g�ɂ��̃C�x���g����������J��
    //�ǂ�����ł��J�ڂł���C�x���g�̏ꍇ��AnyState����C�x���g��T��
    public void Dispatch(int eventID)
    {
        BaseState to;
        if (!CurrentState.transitions.TryGetValue(eventID,out to))
        {
            if (!SearchState<AnyState>().transitions.TryGetValue(eventID, out to)) return;
        }
        TransitionTo(to);
    }

    //���̃X�e�[�g�}�V���̍ŏ��̃X�e�[�g�����߂�
    public void Initialize<T>() where T:BaseState,new()
    {

        CurrentState = SearchState<T>();
    }

    //�X�e�[�g�̃A�b�v�f�[�g����
    public void OnUpdate()
    {
        CurrentState.Update();
    }

    //�X�e�[�g�̒��ړI�ȑJ��
    private void TransitionTo(BaseState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Entry();
    }
}
