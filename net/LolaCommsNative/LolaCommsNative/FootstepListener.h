#pragma once
#ifndef _FOOTSTEP_LISTENER_H
#define _FOOTSTEP_LISTENER_H

///////////////////////////////////////////
// Provides a convenient wrapper around
// communication to receive footstep data
// from Lola
///////////////////////////////////////////

#include <functional>
#include <thread>

#include <iface_ps.hpp>
#include <iface_msg.hpp>
#include <iface_sig.hpp>
#include <iface_sig_msg.hpp>
#include <iface_sig_wpatt.hpp>
#include <iface_stepseq.hpp>

#include "sock_utils.h"
#include "Logging.h"

class FootstepListener
{
public:
    // a subset of the data from am2b_iface::struct_data_stepseq_ssv_log
    typedef struct Footstep
    {
        uint64_t stamp_gen;

        float L0;
        float L1;
        float B;
        float phiO;
        float dz_clear;
        float dz_step;
        float dy;
        float H;
        float T; // step time    

        float start_x;
        float start_y;
        float start_z;
        float start_phi_z;
        float phi_leg_rel;
        int32_t stance;

        Footstep(am2b_iface::struct_data_stepseq_ssv_log* stepseq_ssv_log)
        {
            stamp_gen   = stepseq_ssv_log->stamp_gen;
            L0          = stepseq_ssv_log->L0;
            L1          = stepseq_ssv_log->L1;
            B           = stepseq_ssv_log->B;
            phiO        = stepseq_ssv_log->phiO;
            dz_clear    = stepseq_ssv_log->dz_clear;
            dz_step     = stepseq_ssv_log->dz_step;
            dy          = stepseq_ssv_log->dy;
            H           = stepseq_ssv_log->H;
            T           = stepseq_ssv_log->T;
            start_x     = stepseq_ssv_log->start_x;
            start_y     = stepseq_ssv_log->start_y;
            start_z     = stepseq_ssv_log->start_z;
            phi_leg_rel = stepseq_ssv_log->phi_leg_rel;
            stance      = stepseq_ssv_log->stance;
        }

        Footstep(am2b_iface::struct_data_stepseq_ssv_log stepseq_ssv_log) : Footstep(&stepseq_ssv_log)
        {}
    };

    typedef std::function<void(Footstep step)> OnNewStep;
    typedef std::function<void(std::wstring)>  OnError;
    typedef std::function<void(std::wstring)>  OnConnect;
    typedef std::function<void(std::wstring)>  OnDisconnect;

    FootstepListener(unsigned int port, std::wstring host, bool verbose = false) :
        _port(port), _hostname(host), _verbose(verbose)
    {
        LogInfo(L"Gonna connect to: " + host);
    }

    void listen()
    {
        if (_listening)
            return;

        _socket = create_client_socket(_port, _hostname);

        if (_socket == INVALID_SOCKET)
        {
            cb(_onError, L"Could not connect to host");
            return;
        }

        _listener = std::thread(&FootstepListener::listen_impl, this);
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

    void onError(OnError cb)           { _onError      = cb; }
    void onNewStep(OnNewStep cb)       { _onNewStep    = cb; }
    void onConnect(OnConnect cb)       { _onConnect    = cb; }
    void onDisconnect(OnDisconnect cb) { _onDisconnect = cb; }

private:
    int          _port;
    int          _socket;
    std::wstring _hostname;
    bool         _verbose   = false;
    bool         _listening = false;
    std::thread  _listener;
    size_t       _buflen    = 2048;

    OnNewStep    _onNewStep;
    OnError      _onError;
    OnConnect    _onConnect;
    OnDisconnect _onDisconnect;

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

        if (_verbose)
            LogInfo(std::wstring(L"[footsteps] Attempting to subscribe to footstep data from ") + _hostname);

        am2b_iface::MsgId footstep_sub_id = __DOM_WPATT; //am2b_iface::STEPSEQ_AR_VIZUALIZATION;
        am2b_iface::MsgHeader footstep_sub = { am2b_iface::ps::SIG_PS_SUBSCRIBE, sizeof(am2b_iface::MsgId) };

        size_t sent = send(_socket, (char*)&footstep_sub, sizeof(footstep_sub), 0);
        if (sent <= 0)
            cb(_onError, std::wstring(L"[footsteps] Error sending subscribe request!"));
        sent = send(_socket, (char*)&footstep_sub_id, sizeof(footstep_sub_id), 0);
        if (sent <= 0)
            cb(_onError, std::wstring(L"[footsteps] Error sending footstep sub ID!"));

        if (_verbose)
            LogInfo(std::wstring(L"[footsteps] Subscribed!"));
        cb(_onConnect, _hostname);

        while (_listening)
        {
            std::fill(buf.begin(), buf.end(), 0);
            ssize_t total_received = 0;
            ssize_t header_size = sizeof(am2b_iface::MsgHeader);
            ssize_t step_size = sizeof(am2b_iface::struct_data_stepseq_ssv_log);

            if (_verbose)
            {
                LogInfo(std::wstring(L"[footsteps] Waiting for am2b_iface::MsgHeader (") +
                        std::to_wstring(header_size) +
                        std::wstring(L" bytes)..."));
            }

            while (total_received < header_size)
            {
                int recvd = 0;
                recvd = recv(_socket, &buf[total_received], _buflen - total_received, 0);

                if (recvd == 0) // connection died
                    break;
                if (recvd == -1) // failed to read from socket
                    cb(_onError, L"[footsteps] read() failed!");

                total_received += recvd;

                if (_verbose)
                {
                    LogInfo(std::wstring(L"[footsteps] footstep header) Received ") +
                        std::to_wstring(total_received) +
                        std::wstring(L" total bytes from ") + _hostname);
                }
            }
            if (total_received < header_size)
                break;

            am2b_iface::MsgHeader* header = (am2b_iface::MsgHeader*)buf.data();
            while (total_received < header_size + header->len)
            {
                int recvd = 0;
                recvd = recv(_socket, &buf[total_received], _buflen - total_received, 0);

                if (recvd == 0) // connection died
                    break;
                if (recvd == -1) // failed to read from socket
                    cb(_onError, L"read() failed!");

                total_received += recvd;

                if (_verbose)
                {
                    LogInfo(std::wstring(L"[footsteps] (footstep data) Received ")
                          + std::to_wstring(total_received)
                          + std::wstring(L" total bytes from ") + _hostname.c_str());
                }
            }

            if (header->id != am2b_iface::STEPSEQ_AR_VIZUALIZATION)
            {
                if (header->id == am2b_iface::COM_EOK)
                {
                    LogInfo(std::wstring(L"[footsteps] Received COM_EOK! msg len: ") + std::to_wstring(header->len));
                }
                else
                {
                    std::cout << "[footsteps] Skipping message (type: 0x"
                        << std::hex << header->id << std::dec
                        << ", expecting: 0x" << std::hex << am2b_iface::STEPSEQ_AR_VIZUALIZATION << std::dec
                        << ")" << std::endl;;
                }
                continue;
            }

            am2b_iface::struct_data_stepseq_ssv_log* message = (am2b_iface::struct_data_stepseq_ssv_log*)(buf.data() + header_size);

            cb(_onNewStep, Footstep(message));
        }


        cb(_onDisconnect, _hostname);
        closesocket(_socket);
    }
};


#endif // _FOOTSTEP_LISTENER_H