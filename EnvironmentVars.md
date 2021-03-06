# Environment Variables

### Token_Generator_Shared_Secret
The client sends this when generating JWT tokens (see authentication)

### Token_Symmetric_Key_Base64
The JWT signing key.  Must be exactly 64 bytes.  If not provided, one will 
be generated for you using RNGCryptoServiceProvider to ensure that it is 
cryptographically random.

### Rest_Api_Cors_Origins
Used by CORS.  A comma separated URLs that are allowed to hit the REST API.

### Hubs_Cors_Origins
Used by CORS.  A comma separated URLs that are allowed to hit the Hubs API (listeners).

### Allow_Anonymous
TRUE means that JWT tokens are ignored for the REST & Hubs APIs.  Also 
Token_Generator_Shared_Secret & Token_Symmetric_Key_Base64 are not 
required and not generated for you.

