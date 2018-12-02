# landisProHarvest2Core

#### Runnable Version 1.0

#### How to start

Under Library-Core-master/src/implementation/main/Model.cs

Describe the how to call all the models.

From line 323

....

 - Load succession
 - Load succession Parameters
 - Initialize succession
 - Load disturbances extensions(including harvest)
     - Load disturbanceExtensions parameters
 - Initialize disturbanceExtensions
 - 
/////// not use for me now//////////////////

 - Load otherExtensions(including harvest)	
	 - Load otherExtensions parameters
 - Initialize otherExtensions

//////////////////////////////////////////////////////////
 
 - main loop
	 - Run disExtensions
	 - Run succession
	 - Run other extensions
	 - 
....

#### To do