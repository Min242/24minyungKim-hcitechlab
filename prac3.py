#해야할 것: column개수(157)랑 데이터 개수(156) 맞추기
#pData[1]==1일때 csv 저장되는거 맞는지 제대로 확인
import threading
import UdpComms2 as U
import queue
import csv
import leap
import numpy as np
import pandas as pd
import time

class MyListener(leap.Listener):
    def __init__(self, data_queue):
        super().__init__()
        self.data_queue = data_queue
    def on_tracking_event(self, event):
        #print("inside")
        row = [leap.get_now()]
        if event.hands:
            for hand in event.hands:
                row.extend([hand.palm.position.x, hand.palm.position.y, hand.palm.position.z,
                            hand.palm.velocity.x, hand.palm.velocity.y, hand.palm.velocity.z,
                            hand.palm.normal.x, hand.palm.normal.y, hand.palm.normal.z,
                            hand.palm.direction.x, hand.palm.direction.y, hand.palm.direction.z,
                            hand.palm.orientation.x, hand.palm.orientation.y, hand.palm.orientation.z, hand.palm.orientation.w])
                #print(row[1])
                for finger in hand.digits:
                    for b in range(0,4):
                        bone = finger.bones[b]
                        row.extend([bone.prev_joint.x, bone.prev_joint.y, bone.prev_joint.z,
                                    bone.rotation.x, bone.rotation.y, bone.rotation.z, bone.rotation.w])
                self.data_queue.put(row)
                #print(self.data_queue.get()[1])
        else:
            #row.extend([''] * (156))
            self.data_queue.put(['']*157)
        #self.writer.writerow(row)
        #if data_queue != None:
            #print("Data Saved I guess...")
    def get_data(self):
        return self.data_queue.get()

def processInput(string):
    string_list = string.split()
    return [int(item) for item in string_list]
def MakeFileName(lst):
    return str(lst[2])+"_"+str(lst[3])+".csv"
# def StartLogging(writer):
#     header = ["Time", "Palm Position X", "Palm Position Y", "Palm Position Z", "Palm Velocity X", "Palm Velocity Y", "Palm Velocity Z",
#               "Palm Normal X", "Palm Normal Y", "Palm Normal Z", "Palm2Finger X", "Palm2Finger Y", "Palm2Finger Z",
#               "Palm Orientation X", "Palm Orientation Y", "Palm Orientation Z", "Palm Orientation W"]
#     for i in range(1, 6):
#         for j in range(1, 5):
#             header.extend([f"Finger {i} Bone {j} Position X", f"Finger {i} Bone {j} Position Y",f"Finger {i} Bone {j} Position Z",
#                            f"Finger {i} Bone {j} Rotation X", f"Finger {i} Bone {j} Rotation Y", f"Finger {i} Bone {j} Rotation Z", f"Finger {i} Bone {j} Rotation W"])
#     writer.writerow(header)
def MakeColumns():
    header = ["Time", "Palm Position X", "Palm Position Y", "Palm Position Z", "Palm Velocity X", "Palm Velocity Y", "Palm Velocity Z",
              "Palm Normal X", "Palm Normal Y", "Palm Normal Z", "Palm2Finger X", "Palm2Finger Y", "Palm2Finger Z",
              "Palm Orientation X", "Palm Orientation Y", "Palm Orientation Z", "Palm Orientation W"]
    for i in range(1, 6):
        for j in range(1, 5):
            header.extend([f"Finger {i} Bone {j} Position X", f"Finger {i} Bone {j} Position Y",f"Finger {i} Bone {j} Position Z",
                           f"Finger {i} Bone {j} Rotation X", f"Finger {i} Bone {j} Rotation Y", f"Finger {i} Bone {j} Rotation Z", f"Finger {i} Bone {j} Rotation W"])
    return header
def StopLogging():
    global file
    if file:
        file.close()
        file = None

#data_queue = queue.Queue()
file = None
pData = None
isFirst = True
previousData = None
def code1(data_queue):
    #global data_queue
    global file
    global pData
    global isFirst
    global previousData
    #rows_array = np.empty((0,157))
    rows_array = np.empty((0, 157))
    sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)
    print("sock")
    while True:
        #debugging
        #print("in the true loop")
        # if data_queue.empty():
        #     print("NO DATA")
        # else:
        #     # row = data_queue.get()
        #     # #print("DATA: " + str(row[1]))
        #     # rows_array = np.vstack([rows_array, row])
        #     #print("DATA: "+str(rows_array[-1,1]))
        #     start_time = time.time()
        #     while time.time() - start_time < 30:
        #         row = data_queue.get()
        #         rows_array = np.vstack([rows_array, row])
        #         print("LOGGING HOPEFULLY")
        #     print(rows_array[-1, 1])
        #     columns = MakeColumns()
        #     df = pd.DataFrame(rows_array, columns=columns)
        #     # filename = MakeFileName(pData)
        #     df.to_csv("test1.csv", index=False)
        #     break
        # row = data_queue.get()
        # print(row[1])
        # rows_array = np.vstack([rows_array, row])
        #debugging
        ##########################################
        data = sock.ReceiveData()
        if data != None:
            pData = processInput(data)
            if isFirst and (pData[2], pData[3]) != previousData:
                print("START LOGGING")
                rows_array = np.empty((0, 157))
                if not data_queue.empty():
                    row = data_queue.get()
                    rows_array = np.vstack([rows_array, row])
                #print(rows_array.shape[0])
                print("Logging Hopefully Started")
                isFirst = False
                previousData = (pData[2], pData[3])
                # filename = MakeFileName(pData)
                # with open(filename, 'w', newline='') as file:
                #     writer = csv.writer(file)
                #     StartLogging(writer)
                #     while True:
                #         row = data_queue.get()
                #         if not row:
                #             break
                #         writer.writerow(row)
            elif (pData[2], pData[3])==previousData and not isFirst:
                if not data_queue.empty():
                    row = data_queue.get()
                    rows_array = np.vstack([rows_array, row])
                    print("SAVED DATA:"+str(rows_array[-1,1]))
            elif (pData[2], pData[3]) != previousData:
                print("END")
                columns = MakeColumns()
                df = pd.DataFrame(rows_array, columns = columns)
                filename = MakeFileName(pData)
                df.to_csv(filename, index=False)
                print("file saved")
                isFirst = True
        #######################################

        # pData = processInput(data)
        # print("22")
        # if pData[1] == 0:
        #     print("23")
        #     filename = MakeFileName(pData)
        #     with open(filename, 'w', newline='') as file:
        #         writer = csv.writer(file)
        #         print("1")
        #         StartLogging(writer)
        #         print("2")
        #         while True:
        #             row = data_queue.get()
        #             print("3")
        #             if not row:
        #                 break
        #             writer.writerow(row)
        #             print("4")
        #             file.flush()
        #             print("5")
        # elif pData[1] == 1:
        #     print("6")
        #     StopLogging()
        #     print("7")
def code2(data_queue):
    #global data_queue
    my_listener = MyListener(data_queue)
    connection = leap.Connection()
    connection.add_listener(my_listener)
    with connection.open():
        while True:
            #data_queue= my_listener.get_data()
            pass
# thread1 = threading.Thread(target=code1)
# thread2 = threading.Thread(target=code2)
# thread1.start()
# thread2.start()
def start_threads():
    data_queue = queue.Queue()
    thread1 = threading.Thread(target=code1, args=(data_queue,))
    thread2 = threading.Thread(target=code2, args=(data_queue,))
    thread1.start()
    thread2.start()

start_threads()

# thread1.join()
# thread2.join()

