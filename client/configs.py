import configparser

config = configparser.ConfigParser()
config.read('configs/appsettings.ini')

IP = config["Server"]["IPv4"]
PORT = int(config["Server"]["Port"])
