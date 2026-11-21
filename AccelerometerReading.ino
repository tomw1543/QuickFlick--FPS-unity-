/*
  Library for the MMA8452Q
  By: Jim Lindblom and Andrea DeVore
  SparkFun Electronics

  Do you like this library? Help support SparkFun. Buy a board!
  https://www.sparkfun.com/products/14587

  This sketch uses the SparkFun_MMA8452Q library to initialize
  the accelerometer and stream calcuated x, y, z, acceleration
  values from it (in g units).

  Hardware hookup:
  Arduino --------------- MMA8452Q Breakout
    3.3V  ---------------     3.3V
    GND   ---------------     GND
  SDA (A4) --\/330 Ohm\/--    SDA
  SCL (A5) --\/330 Ohm\/--    SCL

  The MMA8452Q is a 3.3V max sensor, so you'll need to do some
  level-shifting between the Arduino and the breakout. Series
  resistors on the SDA and SCL lines should do the trick.

  License: This code is public domain, but if you see me
  (or any other SparkFun employee) at the local, and you've
  found our code helpful, please buy us a round (Beerware
  license).

  Distributed as is; no warrenty given.
*/

#include <Wire.h>                 // Must include Wire library for I2C
#include "SparkFun_MMA8452Q.h"    // Click here to get the library: http://librarymanager/All#SparkFun_MMA8452Q

MMA8452Q accel(0x1D);                   // create instance of the MMA8452 class
uint8_t button_off_time = 0;
uint8_t button_on = 0;

void setup() {
  Serial.begin(9600);
  // Serial.println("MMA8452Q Basic Reading Code!");
  Wire.begin();
  pinMode(7, OUTPUT); //Error light
  pinMode(5, INPUT); //Button reading
  digitalWrite(7,LOW);

  if (accel.begin() == false) {
    digitalWrite(7,HIGH);
    // Serial.println("Not Connected. Please check connections and read the hookup guide.");
    while (1);
  }
}

void loop() {
  delay(1);

  //debounce the button
  uint8_t previous_button = button_on;
  if (digitalRead(5)){
    button_on = 1;
    button_off_time = 0;
  } else{
    button_off_time++;
  }

  if (button_off_time > 10){ //10ms debounce time
    button_on = 0;
  }
  uint8_t button_rising_edge = button_on && (!previous_button);

  if (accel.available()) {      // Wait for new data from accelerometer
    // Acceleration of x, y, and z directions in g units
    Serial.print(accel.getCalculatedX(), 3);
    Serial.print(",");
    Serial.print(accel.getCalculatedY(), 3);
    Serial.print(",");
    Serial.print(accel.getCalculatedZ(), 3);
    Serial.print(",");
    Serial.print(button_rising_edge);
    Serial.println();
    digitalWrite(7,LOW);
  } else{
    digitalWrite(7, HIGH);
  }
}
