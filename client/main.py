import logging
import socket
import time
import os
import sys

import configs

LOG_DIR = "Logs"
FILE_NAME = "logs.log"
FULL_PATH = os.path.join(LOG_DIR, FILE_NAME)

if not os.path.exists(LOG_DIR):
    os.makedirs(LOG_DIR)

if not os.path.exists(FULL_PATH):
    f = open(FULL_PATH, 'w')
    f.close()


logging.basicConfig(filename=FULL_PATH,
                    filemode='a',
                    format='%(asctime)s %(name)s [%(levelname)s] | message: %(message)s',
                    datefmt='%H:%M:%S',
                    level=logging.INFO)
logger = logging.getLogger()

socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_STREAM)


def main():
    socket.connect((configs.IP, configs.PORT))
    logger.info(f"Client {socket.getsockname()[0]}:{socket.getsockname()[1]} connected to {socket.getpeername()[0]}:{socket.getpeername()[1]}")

    time.sleep(5)
    sendMessage = "Ilin Artem Aleksandrovich M3O-107b-23"
    socket.send(sendMessage.encode())
    logger.info(f"Send message: {sendMessage}")

    data = socket.recv(1024)
    logger.info(f"Server retutn message: {data.decode()}")
    socket.close()



if __name__ == "__main__":
    try:
        logger.debug("Start of program.")
        main()
    except Exception as ex:
        logger.error(ex)
    finally:
        socket.close()
logger.debug("End program.\n")