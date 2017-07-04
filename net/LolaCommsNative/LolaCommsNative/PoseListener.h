#pragma once
#ifndef _POSE_LISTENER_H
#define _POSE_LISTENER_H

////////////////////////////////////////////////
// Provides a convenient wrapper around
// communication to receive pose data from Lola
////////////////////////////////////////////////

#include <functional>
#include <thread>
#include <vector>

#include <iface_vis.h>

#include "Logging.h"
#include "sock_utils.h"

class PoseListener
{
public:
    typedef std::function<void(HR_Pose_Red*)> OnNewPose;
    typedef std::function<void(std::string)>  OnError;

    PoseListener(int port, bool verbose) : _port(port), _verbose(verbose)
    {
    }

    void listen()
    {
        if (_listening)
            return;

        _listening = true;

        _socket = create_udp_socket(_port);
        _listener = std::thread(&PoseListener::listen_impl, this);
    }

    bool listening()
    {
        return _listening;
    }

    void stop()
    {
        if (_listening)
        {
            _listening = false;
            if (_listener.joinable())
                _listener.join();
        }
    }

    void onError(OnError cb)     { _onError   = cb; }
    void onNewPose(OnNewPose cb) { _onNewPose = cb; }
private:
    int         _port;
    int         _socket;
    bool        _verbose   = false;
    bool        _listening = false;
    std::thread _listener;
    size_t      _buflen    = sizeof(HR_Pose_Red) + 1;

    OnNewPose _onNewPose;
    OnError   _onError;

    // wrapper to ensure safe use of callbacks
    template <class Functor, class... Args>
    void cb(Functor&& f, Args&&... args)
    {
        if (f)
            f(std::forward<Args>(args)...);
    }

    void listen_impl()
    {
        std::vector<char> buf;
        buf.resize(_buflen);
        sockaddr_in si_other;
        int slen = sizeof(sockaddr);

        while (_listening)
        {
            // wait for message
            int nrecvd = recvfrom(_socket, buf.data(), _buflen - 1, 0, (sockaddr*)&si_other, &slen);

            if (nrecvd < 0)
            {
                if (errno == EAGAIN || errno == EWOULDBLOCK)
                    continue;
                else
                    cb(_onError, "Unable to receive data");
                continue;
            }

            if (_verbose)
            {
                LogInfo(std::wstring(L"[PoseListener] Received ") + std::to_wstring(nrecvd) + std::wstring(L" bytes from ") + hostString(si_other));
            }

            HR_Pose_Red* new_pose = (HR_Pose_Red*)buf.data();
            cb(_onNewPose, new_pose);
        }
    }
};

#endif // _POSE_LISTENER_H