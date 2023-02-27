//参考サイト様 https://zenn.dev/o8que/books/bdcb9af27bdd7d/viewer/eece5f

using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

// IEnumerable<RoomInfo>インターフェースを実装して、foreachでルーム情報を列挙できるようにする
public class RoomList : IEnumerable<RoomInfo>
{
    private Dictionary<string, RoomInfo> dictionary = new Dictionary<string, RoomInfo>();
    public List<string> roomNameText = new List<string>();
    public List<int> roomMemberCount = new List<int>();
    public List<int> roomMaxPlayers = new List<int>();
    public List<bool> roomIsVisible = new List<bool>();

    public (List<string> rnt, List<int> rmc, List<int> rmp, List<bool> riv) Update(List<RoomInfo> changedRoomList) {
        foreach (var info in changedRoomList) {
            if (!info.RemovedFromList) {
                dictionary[info.Name] = info;
            } else {
                dictionary.Remove(info.Name);
            }
            roomNameText.Add(info.Name);
            roomMemberCount.Add(info.PlayerCount);
            roomMaxPlayers.Add(info.MaxPlayers);
            roomIsVisible.Add(info.IsVisible);
        }
        return (roomNameText, roomMemberCount, roomMaxPlayers, roomIsVisible);
    }

    public void Clear() {
        dictionary.Clear();
    }

    // 指定したルーム名のルーム情報があれば取得する
    public bool TryGetRoomInfo(string roomName, out RoomInfo roomInfo) {
        return dictionary.TryGetValue(roomName, out roomInfo);
    }

    public IEnumerator<RoomInfo> GetEnumerator() {
        foreach (var kvp in dictionary) {
            yield return kvp.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}