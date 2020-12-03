/****************************************************
    文件：IState
    作者：ningwei
    日期：2020/12/3 23:38:6
    功能：状态接口
*****************************************************/
namespace Improve
{
    public interface IState
    {
        void Enter(EntityBase entity, params object[] args);

        void Process(EntityBase entity, params object[] args);

        void Exit(EntityBase entity, params object[] args);
    }

    public enum AniState
    {
        None,
        Born,
        Idle,
        Move,
        Attack,
        Hit,
        Die,
    }
}
