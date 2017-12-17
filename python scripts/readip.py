import commands
import datetime
import requests
import json
import time
from twilio.rest import Client
import ConfigParser

#(<client name>, <client type>, <owner>)
hostnames = [("dacto-fc", "Computer", "Kevin"), ("tangkev11", "Computer", "Kevin"),("blackberry-5064", "Phone", "Kevin"), ("watson", "Laptop", "Paul"), ("huys-applewatch","Huy") ]
config = ConfigParser.ConfigParser()
config.read('config.ini')
HomeMicroserviceBaseUrl = config.get('AWS', 'HomeMicroserviceUrl')
onlinedevices = "|"

def readlocalip():
    #Send GET request for all people
    #Base URL ec2-34-239-110-188.compute-1.amazonaws.com
    #GET /Attendance/People
    GetPeopleResponse = requests.get(HomeMicroserviceBaseUrl + "/Attendance/People")
    if GetPeopleResponse.status_code is 200:
        people = json.loads(GetPeopleResponse.content)
        res = commands.getstatusoutput('arp -a')
        onlinedevicesmessage = "The following devices are online: |"
        if res[0] == 0:
            print res[1]
            for host in hostnames:
                if host[0] in res[1]:
                    #Update db
                    headers = {'Content-Type': 'application/json'}
                    data = {'Name': host[2],'IsHome': True}
                    resp = requests.post(HomeMicroserviceBaseUrl + "/Attendance/Update", headers=headers, json=data)
                    print datetime.datetime.now().strftime("%A, %d. %B %Y %I:%M%p")
                    print host[2] + '\'s ' + host[1] + ' is online'
                    onlinedevicesmessage += host[1]
                    onlinedevicesmessage += "|"
            print onlinedevices
    else:
        print 'Could not get response from GetPeople microservice call'
def pinglocalip():
    commands.getoutput('nmap -sP 192.168.1.*')

def polltime():
    morningstart = datetime.time(6, 0)
    morningend = datetime.time(10,0)
    afternoonstart = datetime.time(12,0)
    afternoonend = datetime.time(22,0)
    present = datetime.datetime.now().time()
    if present>morningstart and present<morningend:
        print 'It is morning'
        return True
    elif present>afternoonstart and present<afternoonend:
        print ' It is afternoon'
        return True
    else:
        print 'Outside colllection time window'
        return False


if __name__ == '__main__':
    while True:
        if polltime():
            print 'Pinging local address range'
            pinglocalip()
            print 'Complete pinging broadcast address.'
            readlocalip()
        time.sleep(15)
