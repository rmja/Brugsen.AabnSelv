# TORMAX TDK Controller 001

![image](tdk-controller-front.jpg)
![image](tdk-controller-back.jpg)
![image](tdk-controller-inside.jpg)

## Door Control

* 1 and 2 closes and locks the door when shorted - the door cannot open before 1 and 2 is released.
* 3 and 4 must have a pulse - then the door opens once in 5 seconds or until the sensor is not active, then it closes and locks.ser.

![image](tdk-controller-input-wiring.png)

## Lock State

The lock state is emitted on the "external comm" RJ12 connector. Pin 4 and 4 are shorted when the door lock is locked; open otherwise.


