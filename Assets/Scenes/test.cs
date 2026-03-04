using UnityEngine;
using Ashutosh.RemoteTuning;
using System.Threading.Tasks;


public class test : MonoBehaviour
{
   async void Start()
    {
       // NOTE: UnityWebRequestTransport is internal, so you can't new it here.
        // We'll create a public factory later (Day 5 wiring).
        Debug.Log("Transport smoke test will be runnable after we expose composition root in Demo.");
        await Task.Yield();
    }
}
