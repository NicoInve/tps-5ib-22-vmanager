# VManager

A C# form manager for Proxmox LXC VMs

## Clone repo
```
git clone https://github.com/NicoInve/tps-5ib-22-vmanager
cd tps-5ib-22-vmanager
```

## Server

To run the server you need Python > v3.

You need also `environs` and `paramiko` packages (you can install them with the requirements.txt file). 

The server needs a `.env` file with `SERVER_HOST`, `SERVER_USERNAME` and `SERVER_PASSWORD` defined with the proper value.
```
cd Server
pip3 install -r requirements.txt
python3 main.py
```
## Client 

To run the client you need to open the visual studio project located in the `Client` folder.

