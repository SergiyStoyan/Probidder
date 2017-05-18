<?php
use OAuth2\Encryption\Jwt; (from composer.json ["bshaffer/oauth2-server-php": "^1.8"] - http://bshaffer.github.io/oauth2-server-php-docs/grant-types/jwt-bearer/)
//GuzzleClient is a curl client for making http requests.

public function getOAuthTAccessToken()
{
    $appUrl = 'https://dev-auth.probidder.com';
    $client = new GuzzleClient($appUrl);

    $privateKey = file_get_contents('win_recording_app.key');
    $client_id   = 'win_recording_app';
    $user_id     = 'WinRecordingApp';
    $grant_type  = 'urn:ietf:params:oauth:grant-type:jwt-bearer';

    $exp = time() + 1000;

    $params = [
        'iss' => $client_id,
        'sub' => $user_id,
        'aud' => $appUrl,
        'exp' => $exp,
        'iat' => time()
    ];

    $jwtUtil = new Jwt();

    $encodedVar =  $jwtUtil->encode($params, $privateKey, 'RS512');

    $request = $client->post('api/oauth/token', null, [
        'grant_type' => $grant_type,
        'assertion' => $encodedVar
    ]);

    $response = $request->send();
    $responseJson = $response->json();

    return $responseJson['access_token'];
}

public function loginByUsername($username, $password)
{
    $accessToken = $this->getOAuthTAccessToken();

    $credentials = [
        'username' => $username,
        'password' => $password,
    ];

    $url = 'https://dev-auth.probidder.com/api/authenticate/recorders/win/app?';
    $params = http_build_query($credentials);

    $client = new GuzzleClient();
    $headers = [
        'Authorization' => 'Bearer ' . $accessToken
    ];

    $request = $client->get($url . $params, $headers);
    $data = [];

    try {
        $response = $request->send();
        $data = $response->json();
    } catch (ClientErrorResponseException $exception) {
        throw $exception;
    }
    /*
    $data contains:
    array:2 [▼
      "status" => true
      "user" => array:16 [▼
        "id" => 53
        "email" => "protest@probidder.com"
        "confirmed" => 1
        "confirmation_code" => null
        "site_id" => null
        "created_at" => "2016-10-25 14:02:30"
        "updated_at" => "2017-05-04 15:06:58"
        "deleted_at" => null
        "username" => "protest"
        "first_name" => "pros"
        "last_name" => "For Test Only"
        "legacy_user_id" => 7745
        "type" => "investor"
        "attributes" => []
        "permissions" => []
        "groups" => []
      ]
    ]
    */
    return $data;
}
