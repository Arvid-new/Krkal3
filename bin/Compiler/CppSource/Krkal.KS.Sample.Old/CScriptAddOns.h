//
// Here you can add your own fields and methods to CScript class
//
// Add variable with points to your service class
// And override the InitializeServices method to initialize the service variable


public:
	CServices *Services;
protected:
	virtual void InitializeServices(void *servicesHandle) {
		Services = (CServices*)servicesHandle;
	}
