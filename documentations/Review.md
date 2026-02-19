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
     - Use asynchronous methods: Async kept for API consistency with a future persistent store; current implementation uses Task.FromResult.
     - Move repository codes to the infrastructure folder + matching namespaces


4. **Bank client implementation & Payment service initialization**

    - Create Bank client options from appsettings files
    - Integrate bank http client
    - Create result feature for better error handling + mapping:

      - Validation → 400 
      - NotFound → 404 
      - SubServiceUnavailable (bank 503) → 503 
      - Internal → 500 
   
    - Initialize Payment service + registration
    - Add Web Application factory for test purposes
    - Update this documentation


5. **Add process payment tests**

    - Mock bank client for test purposes
    - Add new controller and service tests (mostly failing for now)


6. **Payment service implementation**

    - Add payment request validator:

        - Card number: digits only, length 14–19
        - Expiry: month 1–12 + not expired (month precision)
        - Currency: ISO 4217 + supported set (<=3)
        - Amount: integer > 0 (minor units)
        - CVV: digits only, length 3–4      

    - Decouple Payment response from the internal model
    - Change CardNumberLastFour to string to prevent information loss from integer parsing
    - Return a GetPaymentResponse when retrieving a payment
    - Update payment service and controller tests
      
        - Tests are mostly managed with external dependencies mocked like the bank service
        - Internal dependencies like paymentRepository can be used in some test cases and mocked in others


7. **Update doc + global clean up**

   - Add general comments and thoughts documentation
   - global clean-up
   

## Comments & Future Improvements

- As payment processing should be considered an atomic operation, there will be a need to have a way of reconciliation to roll back or retry if issues appear after a payment has already been processed by the bank service.
  
    - Retry policy (Polly) + idempotency key.
    - Event driven consumer/batch ?
  
- Observability (structured logs + correlation id)
- Add authorization / encryption / versioning
