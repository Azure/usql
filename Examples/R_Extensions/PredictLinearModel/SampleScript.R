load("LR_Iris.rda")

output2USQL=predict(lm.fit, inputFromUSQL, interval="confidence")
