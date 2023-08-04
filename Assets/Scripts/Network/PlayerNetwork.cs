using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private readonly NetworkVariable<PlayerNetworkData> _netState = new NetworkVariable<PlayerNetworkData>(writePerm: NetworkVariableWritePermission.Owner);
    private short[] tempMove = null;
    private void Awake()
    {
        ResetNetworkMove();
    }
    private void ResetNetworkMove()
    {
        _netState.Value = new PlayerNetworkData()
        {
            Move = null
        };
    }
    private void Start()
    {
        gameObject.name = "PN " + NetworkObject.NetworkObjectId;
    }
    private void Update()
    {
        if (IsOwner)
        {
            _netState.Value = new PlayerNetworkData()
            {
                Move = tempMove
            };
        }
        else
        {
            var oppMove = _netState.Value.Move;
            if (oppMove != null)
                GameController.Instance.OnOpponentMoves(oppMove);
        }
    }
    public void MoveTo(short[] moveData)
    {
        tempMove = moveData;
    }
    struct PlayerNetworkData : INetworkSerializable
    {
        private short _startX, _startY, _endX, _endY;
        internal short[] Move
        {
            get
            {
                if (_startX == -1)
                    return null;
                return new short[4] { _startX, _startY, _endX, _endY };
            }
            set
            {
                if (value == null)
                {
                    _startX = _startY = _endX = _endY = -1;
                    return;
                }
                _startX = value[0];
                _startY = value[1];
                _endX = value[2];
                _endY = value[3];
            }
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _startX);
            serializer.SerializeValue(ref _startY);
            serializer.SerializeValue(ref _endX);
            serializer.SerializeValue(ref _endY);
        }
    }
}
