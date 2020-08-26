import smbus			#import SMBus module of I2C
from time import sleep          #import
import serial
import string
import pynmea2
import time
from datetime import datetime
from time import gmtime, strftime
import time
import pymysql.cursors
from datetime import date
import math
import urllib3

try:
    import httplib
except:
    import http.client as httplib

def have_internet():
    conn = httplib.HTTPConnection("www.google.com", timeout=5)
    try:
        conn.request("HEAD", "/")
        conn.close()
        return True
    except:
        conn.close()
        return False


#some MPU6050 Registers and their Address
PWR_MGMT_1   = 0x6B
SMPLRT_DIV   = 0x19
CONFIG       = 0x1A
GYRO_CONFIG  = 0x1B
INT_ENABLE   = 0x38
ACCEL_XOUT_H = 0x3B
ACCEL_YOUT_H = 0x3D
ACCEL_ZOUT_H = 0x3F
GYRO_XOUT_H  = 0x43
GYRO_YOUT_H  = 0x45
GYRO_ZOUT_H  = 0x47

#now = datetime.now()
def MPU_Init():
	#write to sample rate register
	bus.write_byte_data(Device_Address, SMPLRT_DIV, 7)
	
	#Write to power management register
	bus.write_byte_data(Device_Address, PWR_MGMT_1, 1)
	
	#Write to Configuration register
	bus.write_byte_data(Device_Address, CONFIG, 0)
	
	#Write to Gyro configuration register
	bus.write_byte_data(Device_Address, GYRO_CONFIG, 24)
	
	#Write to interrupt enable register
	bus.write_byte_data(Device_Address, INT_ENABLE, 1)

def read_raw_data(addr):
	#Accelero and Gyro value are 16-bit
        high = bus.read_byte_data(Device_Address, addr)
        low = bus.read_byte_data(Device_Address, addr+1)
    
        #concatenate higher and lower value
        value = ((high << 8) | low)
        
        #to get signed value from mpu6050
        if(value > 32768):
                value = value - 65536
        return value
http = urllib3.PoolManager()        


bus = smbus.SMBus(1) 	# or bus = smbus.SMBus(0) for older version boards
Device_Address = 0x68   # MPU6050 device address

MPU_Init()

first_run = True
initial_ax = 0
initial_ay = 0
initial_az = 0


while True:
    while(have_internet() == True):
		connection = pymysql.connect(host='35.184.48.158',
         
										    user='root',
											password='101721',
											db='Sistemacarros',
											charset='utf8mb4',
											cursorclass=pymysql.cursors.DictCursor)
														
		port="/dev/ttyAMA0"
		ser=serial.Serial(port , baudrate=9600, timeout=0.5)
		dataout = pynmea2.NMEAStreamReader()
		newdata=ser.readline()

		if newdata[0:6] == "$GPRMC":
			newmsg=pynmea2.parse(newdata)
			lat=newmsg.latitude
			lng=newmsg.longitude
			speed=newmsg.spd_over_grnd
			if not isinstance(speed, float):
				speed
			#print(newmsg.spd_over_grnd)
			print(speed)
			#print(str(speed))
			print(float(speed))
			#print(str(lat))
			#print(str(lng))

			  #  gps = "Latitude=" + str(lat) + "and Longitude=" + str(lng) + "and speed= " + str(speed)

			#Read Accelerometer raw value
			acc_x = read_raw_data(ACCEL_XOUT_H)
			acc_y = read_raw_data(ACCEL_YOUT_H)
			acc_z = read_raw_data(ACCEL_ZOUT_H)
			#Read Gyroscope raw value
			gyro_x = read_raw_data(GYRO_XOUT_H)
			gyro_y = read_raw_data(GYRO_YOUT_H)
			gyro_z = read_raw_data(GYRO_ZOUT_H)

			#Full scale range +/- 250 degree/C as per sensitivity scale factor
			Ax = acc_x/16384.0
			Ay = acc_y/16384.0
			Az = acc_z/16384.0
			Gx = gyro_x/131.0
			Gy = gyro_y/131.0
			Gz = gyro_z/131.0

			if (first_run):
				initial_ax = Ax
				initial_ay = Ay
				initial_az = Az
			acex= (Ax-initial_ax )*(Ax-initial_ax )
			acey= (Ay - initial_ay) * (Ay - initial_ay)
			acez= (Az-initial_az)*(Az-initial_az)
			gforce = math.sqrt(acex+acey+acez)
			g_force = gforce*10
		#	print(gps)
		#	print(initial_ax)
		#	print( "ax=" + str(Ax))
		#	print( "ax=" + str(Ay))
		#	print( "ax=" + str(Az))
		#	print(g_force)
	#		print(strftime("%d:%m:%Y"))
	#		print(strftime("%H:%M:%S"))
	#		print("Ax=%.2f g" %Ax, "Ay=%.2f g" %Ay, "Az=%.2f g" %Az)
#			print("Gx=%.2f g" %Gx,"Gy=%.2f g" %Gy,"Gx=%.2f g" %Gz)
	             	first_run = False
			time.sleep(1)

			with connection.cursor() as cursor:
				sql =  "INSERT INTO `datos` (`idusuario`, `idvehiculo`,`longitud`,`latitud`,`velocidad`, `acex`, `acey`,`acez`,`girox`, `giroy`,`giroz`,`gforce`,`fecha`, `hora`) VALUES ('{}','{}','{}','{}','{}','{}','{}','{}','{}','{}','{}','{}','{}','{}' )".format(1,1,str(lng),str(lat), float(speed) , str(Ax), str(Ay), str(Az), str(Gx), str(Gy), str(Gz), float(g_force) , strftime("%Y-%m:-%d"),strftime("%H:%M:%S"))
				cursor.execute(sql)
				connection.commit()
		
