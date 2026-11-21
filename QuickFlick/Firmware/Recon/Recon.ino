#include <Wire.h>
#include "SparkFun_MMA8452Q.h"

MMA8452Q accel(0x1D);

void setup() {
  Serial.begin(9600);
  Wire.begin();
  pinMode(7, OUTPUT);
  digitalWrite(7, LOW);
  if (accel.begin() == false) {
    digitalWrite(7, HIGH);
    while (1);
  }
}

void loop() {
  if (accel.available()) {
    Serial.print(accel.getCalculatedX(), 3);
    Serial.print(",");
    Serial.print(accel.getCalculatedY(), 3);
    Serial.print(",");
    Serial.print(accel.getCalculatedZ(), 3);
    Serial.println();
    digitalWrite(7, LOW);
  } else {
    digitalWrite(7, HIGH);
  }
}
