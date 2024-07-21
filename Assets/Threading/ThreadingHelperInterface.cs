/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public interface ThreadingHelperInterface<ThreadInfo, RequestParams, Data>
{
    ConcurrentQueue<ThreadInfo> ThreadInfoQueue { get; set; }

    void RequestData(Action<Data> callBack, RequestParams reqParams);

    void OnDataReceived(Data data);

    //this runs on a different thread
    void DataThread(Action<Data> callBack, RequestParams reqParams);
}
*/