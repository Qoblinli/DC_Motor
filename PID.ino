#include <PinChangeInt.h>
#include <PID_v1.h>
#define encodPinA1      2                       // Quadrature encoder A pin
#define encodPinB1      8                       // Quadrature encoder B pin
#define M1              9                       // PWM outputs to L298N H-Bridge motor driver module
#define M2              10

double kp = 5 , ki = 1 , kd = 0.01;             // modify for optimal performance
double input = 0, output = 0, setpoint = 110;
float comdata=0,analog=21;
long temp=0,prev=0;
int isDirection=0,index=0;
int isfirstLoad=1;

volatile long encoderPos =110;
PID myPID(&input, &output, &setpoint, kp, ki, kd, REVERSE);  // if motor will only run at full speed try 'REVERSE' instead of 'DIRECT'

void setup() {
  pinMode(encodPinA1, INPUT_PULLUP);                  // quadrature encoder input A
  pinMode(encodPinB1, INPUT_PULLUP);                  // quadrature encoder input B
  attachInterrupt(0, encoder, FALLING);               // update encoder position
  TCCR1B = TCCR1B & 0b11111000 | 1;                   // set 31KHz PWM to prevent motor noise
  myPID.SetMode(AUTOMATIC);
  myPID.SetSampleTime(1);
  myPID.SetOutputLimits(-255, 255);
  Serial.begin (115200);                              // for debugging
}

void loop() {
 
    if(Serial.available() > 0) {   
      delay(10);                                   // 等待数据传完
      String _temp="";
      _temp=Serial.readStringUntil(',');              //获取串口接收到的数据
      if(_temp=="tozero"){
        _temp="";
        isfirstLoad=1;
        //Serial.println("+++++++++++++++++++++++++++++++++++++++++++++");    
        }else{ 
          comdata=_temp.toFloat();
        }
      Serial.flush();
      analog=comdata; 
     
      Serial.println("Serial.available = :");
      Serial.println(analog);  
      index=0;
      long _sub_val=comdata-prev;
      if(_sub_val>0) {
        isDirection=1;
      }
      if(_sub_val<0) {
        isDirection=2;
      }
      
      prev=comdata; 

       if(isfirstLoad==1){
        isfirstLoad=0;
        setpoint = analog * 5;
        input=setpoint;
        input = encoderPos ; 
        //isDirection=1;
        //encoder();
         if(index>=20)
        {
          //index=0;
          output=0;
          analogWrite(M1, LOW);
          analogWrite(M2, LOW);
          delay(1000);
        }
          myPID.Compute();   
         pwmOut(output);
           
        }
    }
    
    //Serial.println("+++++++++++++++++++++++++++++++++++++++++++++");     
    //Serial.println(isDirection);  
    setpoint = analog * 5;                     // modify to fit motor and encoder characteristics, potmeter connected to A0  
    
    //Serial.println(setpoint);
    input = encoderPos ;                                // data from encoder
    //Serial.println("--------------------------------------------");   
    Serial.println(encoderPos);
    //Serial.println(setpoint);                         // monitor motor position
    
    myPID.Compute();                                    // calculate new output
    if(index>=20)
    {
      //index=0;
      output=0;
      analogWrite(M1, LOW);
      analogWrite(M2, LOW);
      delay(1000);
    }
    pwmOut(output);                                // drive L298N H-Bridge module
    //Serial.println(output);
    //encoder();
}


void pwmOut(int out) {                                // to H-Bridge board
  if (out > 0) {
    analogWrite(M1, out);                             // drive motor CW
    analogWrite(M2, 0);
  }
  else {
    analogWrite(M1, 0);
    analogWrite(M2, abs(out));                        // drive motor CCW
  }
  //Serial.println("--------------------------------------------");   
  //Serial.println(out);   
}

void encoder()  {                                     // pulse and direction, direct port reading to save cycles  5000  -18
  if (isDirection==1)
  {
    if(encoderPos<1145&&abs(encoderPos-setpoint)>=10)
     {
      encoderPos++;             // if(digitalRead(encodPinB1)==HIGH)   count ++;
      index=0;
     }      
    else
      isDirection=0;
  }
  if(isDirection==2){
    if(encoderPos>100&&abs(encoderPos-setpoint)>=10){
      encoderPos--;            // if(digitalRead(encodPinB1)==LOW)   count --;          
      index=0;
    }
    else
      isDirection=0;
  }
  if(isDirection==0){
    index+=1;
      //encoderPos=setpoint;
//    analogWrite(M1, LOW);
//    analogWrite(M2, LOW);
//    
//    delay(1000);
  }
  
//  Serial.println(encoderPos);      
}
