#!/usr/bin/python
import OpenTokSDK

api_key = '11421872' # Replace with your OpenTok API key.
api_secret = '296cebc2fc4104cd348016667ffa2a3909ec636f'  # Replace with your OpenTok API secret.
session_address = '127.0.0.1' # Replace with the representative URL of your session.

opentok_sdk = OpenTokSDK.OpenTokSDK(api_key, api_secret, staging=True)

token = opentok_sdk.generate_token('2_MX4xMTQyMTg3Mn43Mi41LjE2Ny4xMzR-VHVlIEF1ZyAyOCAxNDo1Njo0MiBQRFQgMjAxMn4wLjk3MjIxMTV-', OpenTokSDK.RoleConstants.PUBLISHER, None, None)

print token
