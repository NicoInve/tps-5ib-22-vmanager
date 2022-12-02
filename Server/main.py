from socket import *
import paramiko
from threading import Thread
from environs import Env

env = Env()
env.read_env()

def handler(connectionSocket):
    while True:
        sentence = connectionSocket.recv(1024).decode('utf-8')
        if sentence == '.':
            break
        result = processMessage(sentence)
        connectionSocket.send(result.encode('utf-8'))
    connectionSocket.close()

def processMessage(command):
    global env
    client = paramiko.client.SSHClient()
    client.set_missing_host_key_policy(paramiko.AutoAddPolicy())
    client.connect(env('SERVER_HOST'), username=env('SERVER_USERNAME'), password=env('SERVER_PASSWORD'))
    _stdin, _stdout,_stderr = client.exec_command(command)
    if(_stderr.channel.recv_exit_status() != 0):
        error = _stderr.read().decode('utf-8')
        if(error == ''):
            return 'Si e\' verificato un errore.'
        return 'Errore: ' + error
    result = _stdout.read().decode('utf-8').strip()
    client.close()
    if(result == ''):
        return 'NULL'
    return result
    
serverPort = 12001
serverSocket = socket(AF_INET, SOCK_STREAM)
serverSocket.setsockopt(SOL_SOCKET, SO_REUSEADDR, 1)
serverSocket.bind(('192.168.0.129', serverPort))
serverSocket.listen(1)

while True:
    print('il server è pronto a ricevere')
    connectionSocket, clientAddress = serverSocket.accept()
    print('Connesiono con: ', clientAddress)
    t = Thread(target=handler, args=(connectionSocket,))
    t.start()