using UnityEngine;
using System.Collections.Generic;
public class resourcesLoader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private static resourcesLoader _instance;
    private Dictionary<string,object> _cache=new Dictionary<string,object>();
    
    public static resourcesLoader Instance{
        get{
            return _instance;
        }
    }
    void Awake(){
        if(_instance==null){
            _instance=this;
        }
    }
    public T Load<T>(string resPath) where T:Object{
        if(_cache.TryGetValue(resPath,out object res)){
            return res as T;
        }
        T loadRes=Resources.Load<T>(resPath);
        if(loadRes!=null){
            _cache.Add(resPath,loadRes);
            return loadRes;
        }
        Debug.LogError($"Failed to load resource:{resPath}");
        return null;
    }
    public void Unload(string resPath){
        if(_cache.TryGetValue(resPath,out object res)){
            Resources.UnloadAsset(res as Object);
            _cache.Remove(resPath);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
