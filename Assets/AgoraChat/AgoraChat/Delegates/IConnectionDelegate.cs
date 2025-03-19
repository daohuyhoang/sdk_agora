namespace AgoraChat
{
    /**
     * The connection callback interface.
     */
    public interface IConnectionDelegate
    {
        /**
         * Occurs when the SDK successfully connects to the chat server.
         */
        void OnConnected();

        /**
         * Occurs when the SDK disconnects from the chat server.
         * 
         * The SDK disconnects from the chat server when you log out of the app or a network interruption occurs.
         * 
         */
        void OnDisconnected();

        /**
         *  Occurs when the user logs in to another device with the current account.
         *
         *  @param deviceName The name of another device.
         *  @param info When device B logs in, it kicks device A offline along with the associated extended information.
         */
        void OnLoggedOtherDevice(string deviceName, string info);

        /**
         *  Occurs when the current user account is removed from the server.
         */
        void OnRemovedFromServer();

        /**
         *  Occurs when the current user account is banned.
         */
        void OnForbidByServer();

        /**
         *  Occurs when the user is forced to log out of the current account because the login password is changed.
         */
        void OnChangedIMPwd();

        /**
         *  Occurs when the user is forced to log out of the current account because he or she reaches the maximum number of devices that the user can log in to with the current account.
         */
        void OnLoginTooManyDevice();

        /**
         *  Occurs when the user is forced to log out of the current account from the current device due to login to another device.
         */
        void OnKickedByOtherDevice();

        /**
         *  Occurs when the user is forced to log out of the current account due to an authentication failure.
         */
        void OnAuthFailed();


        /**
         * Occurs when the token has expired.
         */
        void OnTokenExpired();

        /**
         * Occurs when the token is about to expire.
         */
        void OnTokenWillExpire();

        /**
         * The number of active apps has reached the upper limit.
         */
        void OnAppActiveNumberReachLimitation();

        /**
         * Occurs when the client begin to receive offline messages from the server.
         */
        void OnOfflineMessageSyncStart();

        /**
         * Occurs when the client finishes receiving offline messages from the server.
         */
        void OnOfflineMessageSyncFinish();
    }

}
