# REVIEW

## Goal

The goal here is to describe the modifications and explain the thought processes while trying to answer the needs described in [Overview.md](Overview.md)

## Changes

1. **Fix existing controller tests**

   - Get a payment from an ID did not return the expected response but a No Content response
   - Solution: check payment and return a 404 HTTP code if null.
   

2. **Make controller requests and responses immutable + small refacto**

   - Create Post payment endpoint skeleton (TODO in a future commit)
   - Move PaymentStatus Enum and match its namespace accordingly across the application
   - Use records to make requests and responses immutable:
   
     - We want these parts to stay untouched and use internal domain models and entities if needed.
    

3. **PaymentsRepository rework**

    As it is, PaymentsRepository is registered as singleton and directly use a simple List as an in memory collection for the payments.
    It can lead to potential concurrency issues and strongly ties this implementation to the application.
    
   - Solutions:
     
     - Add IPaymentsRepository interface: it allows flexibility if we want to change repository implementations in the future
     - Rename the current PaymentsRepository to InMemoryRepository for clarity.
     - Replace the payment collection by a concurrent dictionary and make it private for safety.
     - Use asynchronous methods
     - Move repository codes to the infrastructure folder + matching namespaces


4. **Bank client implementation & Payment service initialization**

    - Create Bank client options from appsettings files
    - Integrate bank http client
    - Create result feature for better error handling
    - Initialize Payment service + registration
    - Add Web Application factory for test purposes
    - Update this documentation